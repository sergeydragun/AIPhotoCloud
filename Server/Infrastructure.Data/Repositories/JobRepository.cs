using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;

namespace Infrastructure.Data.Repositories;

public class JobRepository : BaseRepository<Job>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context)
    {
    }
}