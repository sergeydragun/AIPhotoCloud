namespace Infrastructure.Data.Entities;

public class Folder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public required string Name { get; set; }
    public required string Path { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
    
    public User User { get; set; }
    public List<FileModel> Files { get; set; }
    public List<Folder> ChildrenFolders { get; set; }
    public Folder? ParentFolder  { get; set; }
    public List<Job>? Jobs { get; set; }
}