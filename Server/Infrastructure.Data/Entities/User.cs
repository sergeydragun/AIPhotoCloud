namespace Infrastructure.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }

    public List<Folder> Folders { get; set; }
    public List<FileModel> Files { get; set; }
    public List<Job> Jobs { get; set; }
    public UserCredentials? Credentials { get; set; }
}