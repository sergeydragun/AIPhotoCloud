namespace Infrastructure.Data.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TargetFileId { get; set; }
    public Guid? TargetFolderId { get; set; }
    public string Payload { get; set; }
    public JobStatus JobStatus { get; set; }
    public int Attempts { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string LastError { get; set; }
    public string? ImpotencyKey { get; set; }
    public string CorrelationId { get; set; }

    public User User { get; set; }
    public FileModel? TargetFile { get; set; }
    public Folder? TargetFolder { get; set; }
}

public enum JobStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}