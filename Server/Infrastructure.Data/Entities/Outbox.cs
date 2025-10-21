namespace Infrastructure.Data.Entities;

public class Outbox
{
    public Guid Id { get; set; }
    public DateTime OccuredAt { get; set; }
    public EventType EventType { get; set; }
    public string PayloadJson { get; set; }
    public bool Processed { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public enum EventType
{
    AddedFile
}