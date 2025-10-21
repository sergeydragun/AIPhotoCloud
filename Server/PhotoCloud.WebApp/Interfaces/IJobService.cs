namespace PhotoCloud.Interfaces;

public interface IJobService
{
    Task CreateProcessingJobForFileAsync(Guid userId, Guid targetFileId);
    
}