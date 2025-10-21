using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data.Interfaces;

public interface IFolderRepository : IBaseRepository<Folder>
{
    Task<List<Folder>> GetBaseUserFolders(Guid userId);
    Task<List<Folder>> GetCurrentFolders(Guid folderId);
    Task<List<FileModel>> GetCurrentFiles(Guid folderId);
}