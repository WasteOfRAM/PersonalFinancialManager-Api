namespace PersonalFinancialManager.Core.Interfaces.Repositories;

using System.Linq.Expressions;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false);

    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false, int page = 1, int itemsPerPage = 0, string? order = null, string? orderBy = null);

    Task AddAsync(TEntity entity);

    void Delete(TEntity entity);

    void Update(TEntity entity);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression);

    Task<int> SaveAsync();
}
