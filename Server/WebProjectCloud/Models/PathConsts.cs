using System.Drawing;
using System.Drawing.Drawing2D;
using WebProjectCloud.YoloParser;
using WebProjectCloud.DataStructures;
using WebProjectCloud;
using Microsoft.ML;

namespace WebProjectCloud.Models
{
    public class PathConsts
    {
        public string DataPath { get; set; }
        public string ModelFilePath { get; set; }
        public string ImageFolder { get; set; }
        public MLContext MlContext { get; set; }

        public PathConsts() 
        {
            DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Photos");

            var assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
            ModelFilePath = Path.Combine(assetsPath, "Model", "TinyYolo2_model.onnx");

            ImageFolder = DataPath;
            MlContext = new MLContext();
        }

        string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

    }
}
