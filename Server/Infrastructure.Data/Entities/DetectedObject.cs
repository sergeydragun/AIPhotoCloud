namespace Infrastructure.Data.Entities;

public class DetectedObject
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public required string Label { get; set; }
    public float Confidence { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public FileModel FileModel { get; set; }
}