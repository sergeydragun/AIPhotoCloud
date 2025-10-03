using AI.Core.Configurations;
using AI.Core.DataStructures;
using AI.Core.Interfaces;
using AI.Core.YoloParser;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace AI.Core.Services
{
    public class ObjectDetection : IObjectDetection
    {
        private readonly MLSettings _settings;

        public ObjectDetection(IOptions<MLSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SetInDbAsync(CancellationToken ct = default)
        {
            MLContext mlContext = new MLContext();

            var folders = _context.FolderModel.ToList();

            try
            {
                foreach(var folder in folders)
                {
                    var files = _context.Files.Where(q => q.FolderModel.Id == folder.Id).ToList();

                    var images = files.Select(f => new ImageNetData { ImagePath = f.Path, Label = f.Name })
                        .ToList();

                    IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);

                    var modelScorer = new OnnxModelScorer(path.ImageFolder, path.ModelFilePath, mlContext);
                    var probabilities = modelScorer.Score(imageDataView);

                    YoloOutputParser parser = new YoloOutputParser();

                    var boundingBoxes =
                        probabilities
                        .Select(probability => parser.ParseOutputs(probability))
                        .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));

                    int count = _context.Files.Count();

                    for (int i = 0; i < count; i++)
                    {
                        var file = files.ElementAt(i);
                        var fileName = file.Name;
                        var detectedObjects = boundingBoxes.ElementAt(i);
                        string countInfo = String.Join(", ", detectedObjects.GroupBy(p => p.Label).Select(p => $"{p.Key} : {p.Count()}").ToList());
                        file.CountsInformation = countInfo;
                        _context.Files.Update(file);                    
                    }

                    folder.InFilesCountsInformation = String.Join(", ", boundingBoxes
                        .SelectMany(f => f)
                        .GroupBy(q => q.Label)
                        .Select(p => $"{p.Key} : {p.Count()}").ToList());

                    _context.FolderModel.Update(folder);
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
