using Microsoft.ML;

namespace AI.Core.Interfaces;

public interface IOnnxModelScorer
{
    IEnumerable<float[]> Score(IDataView data);
}