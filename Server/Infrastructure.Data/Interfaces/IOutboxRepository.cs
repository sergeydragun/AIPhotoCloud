using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data.Interfaces;

public interface IOutboxRepository : IBaseRepository<Outbox>
{
    
}