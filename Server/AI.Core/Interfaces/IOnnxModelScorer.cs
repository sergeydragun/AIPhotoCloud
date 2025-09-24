using Microsoft.ML;

namespace PhotoCloud.Models;

public interface IOnnxModelScorer
{
    IEnumerable<float[]> Score(IDataView data);
}