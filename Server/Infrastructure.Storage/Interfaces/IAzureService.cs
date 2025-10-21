using Azure.Storage.Blobs.Models;
using CSharpFunctionalExtensions;

namespace Infrastructure.Storage.Interfaces;

public interface IAzureService
{
    Task EnsureContainerExistsAsync();
    Uri GetUploadUrlSas(string filePath, int minutes = 30);

    Task<(Stream Stream, string ContentType)?> GetImageAsync(string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> BlobExistsAsync(string blobName, CancellationToken ct = default);
    Task<BlobProperties?> GetBlobPropertiesAsync(string blobName, CancellationToken ct = default);
    Task<Result> ValidateBlobPropertiesAsync(string blobName, long expectedSizeBytes, CancellationToken ct = default);
}