using Infrastructure.Data.Entities;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;

    public JobService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task CreateProcessingJobForFileAsync(
        Guid userId,
        Guid targetFileId)
    {
        var job = new Job()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetFileId = targetFileId,
            JobStatus = JobStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _jobRepository.Create(job);
        await _jobRepository.SaveChangesAsync();
    }
}