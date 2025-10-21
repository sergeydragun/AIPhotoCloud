using Infrastructure.Data.DbContext;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Interfaces;

namespace Infrastructure.Data.Repositories;

public class UserCredentialsRepository : BaseRepository<UserCredentials>, IUserCredentialsRepository
{
    public UserCredentialsRepository(AppDbContext context) : base(context)
    {
    }
}