using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class FolderRepository : BaseRepository<Folder>, IFolderRepository
{
    public FolderRepository(AppDbContext context) : base(context)
    {
    }

    public Task<List<Folder>> GetBaseUserFolders(Guid userId)
    {
        return _context
            .Users
            .Include(u => u.Folders)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Folders)
            .Where(f => f.ParentFolderId == null)
            .ToListAsync();
    }

    public Task<List<Folder>> GetCurrentFolders(Guid folderId)
    {
        return _context
            .Folders
            .Where(f => f.ParentFolderId == folderId)
            .ToListAsync();
    }

    public Task<List<FileModel>> GetCurrentFiles(Guid folderId)
    {
        return _context
            .Files
            .Where(f => f.FolderId == folderId)
            .ToListAsync();
    }
}