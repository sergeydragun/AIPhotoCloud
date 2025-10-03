using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CSharpFunctionalExtensions;
using Infrastructure.Storage.Configuration;
using Infrastructure.Storage.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage.Services;

public class AzureService : IAzureService
{
    private readonly AzureStorageConfiguration _cfg;
    private readonly BlobServiceClient _serviceClient;

    public AzureService(IOptions<AzureStorageConfiguration> azureStorageConfiguration)
    {
        _cfg =  azureStorageConfiguration.Value;
        _serviceClient = new BlobServiceClient(azureStorageConfiguration.Value.ConnectionString);
        
        
    }
    
    public async Task EnsureContainerExistsAsync()
    {
        var containerClient = _serviceClient.GetBlobContainerClient(_cfg.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
    }

    public Uri GetUploadUrlSas(string filePath, int minutes = 30)
    {
        var blobUri = new Uri($"{_cfg.AzureHost}/{_cfg.AccountName}/{_cfg.ContainerName}/{filePath}");

        var credential = new StorageSharedKeyCredential(_cfg.AccountName, _cfg.AccountKey);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = _cfg.ContainerName,
            BlobName = filePath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(minutes)
        };
        
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Create | BlobAccountSasPermissions.Write);
        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        
        var uploadUri = new UriBuilder(blobUri) { Query = sasToken }.Uri;
        
        return uploadUri;
    }
    
    public async Task<(Stream Stream, string ContentType)?> GetImageAsync(string blobName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _serviceClient.GetBlobContainerClient(_cfg.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists)
            return null;

        var download = await blobClient.DownloadAsync(cancellationToken);
        var ms = new MemoryStream();
        await download.Value.Content.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        string contentType = download.Value.ContentType;
        return (ms, contentType);
    }
    
    public async Task<bool> BlobExistsAsync(string blobName, CancellationToken ct = default)
    {
        var containerClient = _serviceClient.GetBlobContainerClient(_cfg.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var exists = await blobClient.ExistsAsync(ct);
        return exists.Value;
    }
    
    public async Task<BlobProperties?> GetBlobPropertiesAsync(string blobName, CancellationToken ct = default)
    {
        var containerClient = _serviceClient.GetBlobContainerClient(_cfg.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        try
        {
            var response = await blobClient.GetPropertiesAsync(cancellationToken: ct);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<Result> ValidateBlobPropertiesAsync(string blobName, long expectedSizeBytes, CancellationToken ct = default)
    {
        var blobProperties = await GetBlobPropertiesAsync(blobName, ct);

        if (expectedSizeBytes == blobProperties!.ContentLength)
        {
            return Result.Success();
        }
        
        return Result.Failure("Blob properties validation failed");
    }
}