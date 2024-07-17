namespace PersonalFinancialManager.Core.Interfaces.Repositories;

using System.Linq.Expressions;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false);

    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false);

    Task AddAsync(TEntity entity);

    void Delete(TEntity entity);

    void Update(TEntity entity);

    Task<int> SaveAsync();
}
