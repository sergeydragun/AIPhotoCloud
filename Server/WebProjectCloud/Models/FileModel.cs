namespace WebProjectCloud.Models
{
    public class FileModel
    {
        public int  Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string? CountsInformation { get; set; }
        public FolderModel? FolderModel { get; set; }
    }
}
