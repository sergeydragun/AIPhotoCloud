using AI.Core.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using PhotoCloud.DataStructures;
using PhotoCloud.YoloParser;

namespace PhotoCloud.Models
{
    public struct ImageNetSettings
    {
        public const int imageHeight = 416;
        public const int imageWidth = 416;
    }

    public struct TinyYoloModelSettings
    {
        public const string ModelInput = "image";

        public const string ModelOutput = "grid";
    }

    public class OnnxModelScorer : IOnnxModelScorer
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _mlModel;
        private readonly MLSettings _settings;

        private IList<YoloBoundingBox> _boundingBoxes = new List<YoloBoundingBox>();

        public OnnxModelScorer(IOptions<MLSettings> settings)
        {
            _settings = settings.Value;
            _mlContext = new MLContext();
        }

        private ITransformer LoadModel(string modelLocation)
        {
            var data = _mlContext.Data.LoadFromEnumerable(new List<ImageNetData>());
            var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(ImageNetData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "image", imageWidth: ImageNetSettings.imageWidth, 
                    imageHeight: ImageNetSettings.imageHeight, inputColumnName: "image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "image"))
                .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { 
                    TinyYoloModelSettings.ModelOutput }, inputColumnNames: new[] { TinyYoloModelSettings.ModelInput}));
            var model = pipeline.Fit(data);
            return model;
        }

        private IEnumerable<float[]> PredictDataUsingModel(IDataView testData, ITransformer model)
        {
            IDataView scoredData = model.Transform(testData);

            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>(TinyYoloModelSettings.ModelOutput);

            return probabilities;
        }

        public IEnumerable<float[]> Score(IDataView data)
        {
            var model = LoadModel(modelLocation);

            return PredictDataUsingModel(data, model);
        }
    }
}
