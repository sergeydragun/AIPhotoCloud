using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;

namespace Infrastructure.Data.Repositories;

public class DetectedObjectsRepository : BaseRepository<DetectedObject>, IDetectedObjectsRepository
{
    public DetectedObjectsRepository(AppDbContext context) : base(context)
    {
    }
}