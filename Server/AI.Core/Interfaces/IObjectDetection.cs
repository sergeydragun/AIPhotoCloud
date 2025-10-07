namespace AI.Core.Interfaces
{
    public interface IObjectDetection
    {
        Task SetInDbAsync(CancellationToken ct = default);
    }
}