using System.Text.Json;
using System.Text.Json.Nodes;
using Infractructure.DTO.WebAppClientDTO;
using Infrastructure.Data.Entities;

namespace PhotoCloud.Interfaces;

public interface IFileFolderService
{
    Task CreateFolderAsync(Guid? parentFolderId, Guid userId, string folderName);

    Task<Uri> CreateURlForFileAsync(Guid? parentFolderId, Guid userId, string fileName, string contentType,
        int expectedSizeBytes);

    Task<List<Folder>> GetFoldersAsync(Guid? parentFolderId, Guid userId);
    Task<List<FileModel>> GetFilesAsync(Guid? parentFolderId, Guid userId);
    Task<JsonObject> ProcessBlobEvent(JsonElement eventsJson);
    Task<CompleteUploadResultDto> CompleteUploadAsync(Guid fileId, string idempotencyKey, Guid currentUserId, CancellationToken cancellationToken);
}