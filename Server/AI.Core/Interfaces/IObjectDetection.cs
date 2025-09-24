namespace PhotoCloud.Models
{
    public interface IObjectDetection
    {
        Task SetInDbAsync(CancellationToken ct = default);
    }
}
