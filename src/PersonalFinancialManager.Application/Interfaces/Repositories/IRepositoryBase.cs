namespace PersonalFinancialManager.Application.Interfaces.Repositories;

using PersonalFinancialManager.Application.Queries;
using System.Linq.Expressions;

public interface IRepositoryBase<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id);

    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, bool asNoTracking = false, string? includeProperty = null);

    Task<QueryResult<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, bool asNoTracking = false, string? includeProperty = null,
        int page = 1, int? itemsPerPage = null, string? order = null, string? orderBy = null);

    Task AddAsync(TEntity entity);

    void Delete(TEntity entity);

    void Update(TEntity entity);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression);

    Task<int> SaveAsync();
}
