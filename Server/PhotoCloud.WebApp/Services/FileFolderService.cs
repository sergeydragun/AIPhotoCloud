using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpFunctionalExtensions;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Repositories;
using Infrastructure.Storage.Interfaces;
using PhotoCloud.Interfaces;
using Serilog;

namespace PhotoCloud.Services;

public class FileFolderService : IFileFolderService
{
    private readonly IFileModelRepository _fileModelRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAzureService _azureService;
    private readonly IJobService _jobService;
    private readonly ILogger _logger;

    public FileFolderService(IFileModelRepository fileModelRepository,
        IFolderRepository folderRepository,
        IUserRepository userRepository,
        IAzureService azureService,
        IJobService jobService,
        ILogger logger)
    {
        _fileModelRepository = fileModelRepository;
        _folderRepository = folderRepository;
        _userRepository = userRepository;
        _azureService = azureService;
        _jobService = jobService;
        _logger = logger;
    }

    public async Task CreateFolderAsync(Guid? parentFolderId, Guid userId, string folderName)
    {
        var user = await _userRepository.FindById(userId);

        var parentFolder = parentFolderId == null
            ? null
            : await _folderRepository.FindById(parentFolderId ?? Guid.Empty);

        var newFolder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = folderName,
            Path = parentFolder == null
                ? $"{user.Id.ToString()}/{folderName}"
                : $"{parentFolder.Path}/{folderName}",
            CreatedOn = DateTime.UtcNow,
            ModifiedOn = DateTime.UtcNow,

            UserId = userId,
            ParentFolderId = parentFolderId
        };

        _folderRepository.Create(newFolder);
    }

    public async Task<Uri> CreateURlForFileAsync(Guid? parentFolderId,
        Guid userId,
        string fileName,
        string contentType,
        int expectedSizeBytes)
    {
        var parentFolder = parentFolderId == null
            ? null
            : await _folderRepository.FindById(parentFolderId ?? Guid.Empty);

        var filePath = parentFolder == null ? $"{userId.ToString()}/{fileName}" : $"{parentFolder.Path}/{fileName}";

        var url = _azureService.GetUploadUrlSas(filePath);

        var file = new FileModel()
        {
            Id = Guid.NewGuid(),
            FolderId = parentFolderId,
            UserId = userId,
            FileStatus = FileStatus.PendingUpload,
            FileName = fileName,
            BlobUri = filePath,
            ContentType = contentType,
            CreatedOn = DateTime.UtcNow,
            ModifiedOn = DateTime.UtcNow,
            ExpectedSizeBytes = expectedSizeBytes
        };

        _fileModelRepository.Create(file);
        await _fileModelRepository.SaveChangesAsync();

        return url;
    }

    public Task<List<Folder>> GetFoldersAsync(Guid? parentFolderId, Guid userId)
    {
        if (parentFolderId == null)
        {
            return _folderRepository
                .GetBaseUserFolders(userId);
        }

        return _folderRepository.GetCurrentFolders(parentFolderId ?? Guid.Empty);
    }

    public Task<List<FileModel>> GetFilesAsync(Guid? parentFolderId, Guid userId)
    {
        return _folderRepository.GetCurrentFiles(parentFolderId ?? Guid.Empty);
    }

    public async Task<JsonObject> ProcessBlobEvent(JsonElement eventsJson)
    {
        foreach (var ev in eventsJson.EnumerateArray())
        {
            var eventType = ev.GetProperty("eventType").GetString();
            if (eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                var data = ev.GetProperty("data");
                var validationCode = data.GetProperty("validationCode").GetString();
                return new JsonObject()
                {
                    ["validationResponse "] = validationCode
                };
            }

            if (eventType == "Microsoft.Storage.BlobCreated")
            {
                var data = ev.GetProperty("data");
                var url = data.GetProperty("url").GetString();
                var blobUrl = new Uri(url);
                var blobName = Uri.UnescapeDataString(blobUrl.AbsolutePath.TrimStart('/').Split('/', 2)[1]);

                var file = await _fileModelRepository.FindByBlobPathAsync(blobName);
                if (file == null)
                {
                    _logger.Warning("BlobCreated for unknown blob {Blob}", blobName);
                    continue;
                }

                var properties = await _azureService.GetBlobPropertiesAsync(blobName);
                if (properties == null)
                {
                    _logger.Warning("Blob not found after notification {Blob}", blobName);
                    continue;
                }

                if (file.ExpectedSizeBytes != properties.ContentLength)
                {
                    _logger.Warning("Size mismatch for {Blob}", blobName);
                    continue;
                }
            }
        }

        return new();
    }

    public async Task<Result> ProcessCompleteUpload(Guid userId, Guid fileId)
    {
        var file = await _fileModelRepository.FindById(fileId);

        if (file.UserId != userId)
            return Result.Failure("File not uploaded");

        var blobResult = await _azureService.ValidateBlobPropertiesAsync(file.BlobUri, file.ExpectedSizeBytes);
        if (blobResult.IsFailure)
            return Result.Failure(blobResult.Error);

        await _jobService.CreateProcessingJobForFileAsync(userId, fileId);

        return Result.Success();
    }

    private bool ValidatePossibilityOfChanging(User user, Folder parentFolder)
    {
        return user.Id == parentFolder.UserId;
    }
}