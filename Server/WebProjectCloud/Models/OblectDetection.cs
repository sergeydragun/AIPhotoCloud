using Microsoft.ML;
using System.Drawing;
using WebProjectCloud.DataStructures;
using WebProjectCloud.YoloParser;

namespace WebProjectCloud.Models
{
    public class OblectDetection : IObjectDetection
    {
        private ApplicationContext _context;
        public OblectDetection(ApplicationContext contect) 
        {
            _context = contect;
        }

        public void SetInDb()
        {
            MLContext mlContext = new MLContext();
            var path = new PathConsts();

            try
            {
                foreach(var folder in _context.FolderModel)
                {
                    var images = _context.Files.Where(q => q.FolderModel.Id == folder.Id)
                        .Select(f => new ImageNetData { ImagePath = f.Path, Label = f.Name })
                        .ToList();
                    IDataView imageDataView = mlContext.Data.LoadFromEnumerable(images);

                    var modelScorer = new OnnxModelScorer(path.ImageFolder, path.ModelFilePath, mlContext);
                    var probabilities = modelScorer.Score(imageDataView);

                    YoloOutputParser parser = new YoloOutputParser();

                    var boundingBoxes =
                        probabilities
                        .Select(probability => parser.ParseOutputs(probability))
                        .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));

                    for (int i = 0; i < _context.Files.Count(); i++)
                    {
                        var fileName = _context.Files.ElementAt(i).Name;
                        var detectedObjects = boundingBoxes.ElementAt(i);
                        string countInfo = String.Join(", ", detectedObjects.GroupBy(p => p.Label).Select(p => $"{p.Key} : {p.Count()}").ToList());
                        _context.Files.ElementAt(i).CountsInformation = countInfo;                     
                    }

                    folder.InFilesCountsInformation = String.Join(", ", boundingBoxes
                        .SelectMany(f => f)
                        .GroupBy(q => q.Label)
                        .Select(p => $"{p.Key} : {p.Count()}").ToList());

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
