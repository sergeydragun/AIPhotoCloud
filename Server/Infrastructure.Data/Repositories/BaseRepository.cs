using System.Linq.Expressions;
using Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public void Create(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public Task<TEntity> FindById(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FindAsync([id], cancellationToken).AsTask()!;
    }

    public IQueryable<TEntity> FindAll()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate).AsQueryable();
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}