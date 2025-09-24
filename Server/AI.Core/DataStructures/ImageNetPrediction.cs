using Microsoft.ML.Data;

namespace PhotoCloud.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
