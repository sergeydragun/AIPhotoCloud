namespace WebProjectCloud.Models
{
    public class FolderModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? InFilesCountsInformation { get; set; }
        public List<FileModel>? FileModels { get; set; }
    }
}
