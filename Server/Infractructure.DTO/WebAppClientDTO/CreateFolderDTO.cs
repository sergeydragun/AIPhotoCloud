namespace Infractructure.DTO.WebAppClientDTO;

public record CreateFolderDTO(
    string Name,
    Guid? ParentFolderId
);

public record CreateFolderResponseDTO(
    Guid FolderId,
    string Name,
    Guid? ParentFolderId,
    string Path,
    DateTime CreatedAt
);