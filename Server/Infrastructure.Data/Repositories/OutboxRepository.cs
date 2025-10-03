using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;

namespace Infrastructure.Data.Repositories;

public class OutboxRepository : BaseRepository<Outbox>, IOutboxRepository
{
    public OutboxRepository(AppDbContext context) : base(context)
    {
    }
}