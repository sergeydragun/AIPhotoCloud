using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpFunctionalExtensions;
using Infractructure.DTO.WebAppClientDTO;
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

    public async Task<CompleteUploadResultDto?> CompleteUploadAsync(
        Guid fileId,
        string? idempotencyKey,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileModelRepository.FindById(fileId, cancellationToken);

        if (file == null)
            throw new FileNotFoundException($"File with id '{fileId}' not found.");

        if (file.UserId != userId)
            throw new UnauthorizedAccessException("User is not owner of the file.");

        if (file.FileStatus == FileStatus.Processing || file.FileStatus == FileStatus.Processed)
        {
            var existingJob = await _db.Jobs
                .Where(j => j.TargetFileId == fileId)
                .OrderByDescending(j => j.SubmittedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return new CompleteUploadResultDto
            {
                FileId = file.Id,
                FileStatus = file.FileStatus.ToString(),
                JobId = existingJob?.Id,
                Message = existingJob != null ? "Job already exists for file." : "File already processed/processing."
            };
        }

        var properties = await _azureService.GetBlobPropertiesAsync(file.BlobUri, cancellationToken);
        if (properties == null)
        {
            throw new InvalidOperationException("Uploaded blob not found in storage.");
        }

        if (file.ExpectedSizeBytes != properties.ContentLength)
        {
            _logger.Warning("Size mismatch for file {FileId}: expected {Expected} actual {Actual}", fileId, file.ExpectedSizeBytes, properties.ContentLength);
            throw new InvalidOperationException("Uploaded blob size does not match expected size.");
        }
        
        using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                Type = JobType.FileProcessing,     
                TargetFileId = file.Id,
                Status = JobStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                IdempotencyKey = idempotencyKey
            };

            file.Status = FileStatus.Processing;
            file.ProcessingStartedAt = DateTime.UtcNow;
            file.ModelVersion = file.ModelVersion ?? "default"; 

            _db.Jobs.Add(job);
            _db.Files.Update(file);

            var eventPayload = new
            {
                jobId = job.Id,
                ownerId = job.OwnerId,
                fileId = file.Id,
                blobPath = file.BlobPath,
                modelVersion = file.ModelVersion,
                submittedAt = job.SubmittedAt
            };

            var outbox = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                Type = "JobCreated",
                Payload = JsonSerializer.Serialize(eventPayload),
                Processed = false
            };
            _db.OutboxMessages.Add(outbox);

            await _db.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);

            return new CompleteUploadResultDto
            {
                FileId = file.Id,
                FileStatus = file.Status.ToString(),
                JobId = job.Id,
                Message = "File validated and job queued."
            };
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }


    private bool ValidatePossibilityOfChanging(User user, Folder parentFolder)
    {
        return user.Id == parentFolder.UserId;
    }
}