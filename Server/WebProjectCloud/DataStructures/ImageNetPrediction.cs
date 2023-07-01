using Microsoft.ML.Data;

namespace WebProjectCloud.DataStructures
{
    public class ImageNetPrediction
    {
        [ColumnName("grid")]
        public float[] PredictedLabels;
    }
}
