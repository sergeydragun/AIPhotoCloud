namespace Infractructure.DTO.WebAppWorkerDTO;

public record JobMessage(
    Guid JobId,
    Guid OwnerId,
    List<Guid> FileIds,
    List<Guid> FileUris,
    DateTime SubmittedAt,
    string CorrelationId,
    string ImpotencyKey
    );
    