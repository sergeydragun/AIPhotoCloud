using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class FileModelRepository : BaseRepository<FileModel>, IFileModelRepository
{
    public FileModelRepository(AppDbContext context) : base(context)
    {
    }


    public Task<List<FileModel>> GetBaseUserFiles(Guid userId)
    {
        return _context
            .Users
            .Include(u => u.Files)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Files)
            .Where(f => f.FolderId == null)
            .ToListAsync();
    }

    public Task<FileModel?> FindByBlobPathAsync(string blobName)
    {
        return _context.Files.FirstOrDefaultAsync(f => f.BlobUri == blobName);
    }
}