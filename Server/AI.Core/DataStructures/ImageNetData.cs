using Microsoft.ML.Data;

namespace AI.Core.DataStructures
{
    public class ImageNetData
    {
        [LoadColumn(0)] public string ImagePath;

        [LoadColumn(1)] public string Label;
    }
}