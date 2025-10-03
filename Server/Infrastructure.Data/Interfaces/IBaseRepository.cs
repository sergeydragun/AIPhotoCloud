using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    void Create(TEntity entity);
    Task<TEntity> FindById(Guid id);
    IQueryable<TEntity> FindAll();
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    void Remove(TEntity entity);
    void Update(TEntity entity);
    void SaveChanges();
    Task SaveChangesAsync();
}