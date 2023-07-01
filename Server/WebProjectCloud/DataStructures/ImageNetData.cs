using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;
using WebProjectCloud.Models;

namespace WebProjectCloud.DataStructures
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;

        public static IEnumerable<ImageNetData> ReadFromFile(string imageFolder, ApplicationContext context)
        {
            return context.Files.Select(f => new ImageNetData { ImagePath = f.Path, Label = f.Name }).ToList();
        }
    }
}
