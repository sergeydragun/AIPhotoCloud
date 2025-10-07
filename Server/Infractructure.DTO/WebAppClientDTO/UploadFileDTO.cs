namespace Infractructure.DTO.WebAppClientDTO;

public record RequestUploadDTO(
    Guid? FolderId,
    string FileName,
    string ContentType,
    int SizeBytes,
    string ImpotencyKey,
    bool ProcessNow
);

public record ResponseUploadDTO(
    Guid FileId,
    string UploadUrl,
    DateTime UploadUrlExpiredAt,
    string BlobPath,
    string Status
);

public record CompleteUploadDTO(
    Guid FileId,
    string ImpotencyKey
);