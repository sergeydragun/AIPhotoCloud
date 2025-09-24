using Microsoft.ML.Data;
using PhotoCloud.Models;

namespace PhotoCloud.DataStructures
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}
