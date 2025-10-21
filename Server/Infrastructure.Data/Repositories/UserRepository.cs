using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;

namespace Infrastructure.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }
}