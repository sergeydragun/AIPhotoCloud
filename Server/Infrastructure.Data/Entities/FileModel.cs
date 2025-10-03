namespace Infrastructure.Data.Entities;

public class FileModel
{
    public Guid Id { get; set; }
    public Guid? FolderId { get; set; }
    public Guid UserId { get; set; }
    public required string FileName { get; set; }
    public required string BlobUri { get; set; }
    public required string ContentType { get; set; }
    public long ExpectedSizeBytes { get; set; }
    public FileStatus FileStatus { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    
    public User User { get; set; }
    public Folder? Folder { get; set; }
    public List<DetectedObject> DetectedObjects { get; set; }
    
    public List<Job> Jobs { get; set; }
}

public enum FileStatus
{
    PendingUpload,
    Uploaded,
    Processing,
    Processed,
    Failed
}