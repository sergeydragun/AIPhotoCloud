using Infrastructure.Data.Repositories;

namespace Infrastructure.Data.Entities;

public interface IFileModelRepository : IBaseRepository<FileModel>
{
    Task<List<FileModel>> GetBaseUserFiles(Guid userId);
    Task<FileModel?> FindByBlobPathAsync(string blobName);
}