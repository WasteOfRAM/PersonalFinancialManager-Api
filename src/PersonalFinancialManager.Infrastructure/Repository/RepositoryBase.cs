namespace PersonalFinancialManager.Infrastructure.Repository;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Interfaces.Repositories;
using PersonalFinancialManager.Infrastructure.Data;
using PersonalFinancialManager.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    private readonly AppDbContext dbContext;
    protected DbSet<TEntity> DbSet { get; set; }

    public RepositoryBase(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, bool asNoTracking = false)
    {
        return asNoTracking ? await DbSet.AsNoTracking().FirstOrDefaultAsync(filter) :
                              await DbSet.FirstOrDefaultAsync(filter);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null,
        bool asNoTracking = false, int page = 1, int? itemsPerPage = null, string? order = null, string? orderBy = null)
    {
        var items = filter is not null ? DbSet.Where(filter) : DbSet.AsQueryable();

        items = items.OrderBy(orderBy, order);

        if (itemsPerPage is not null && itemsPerPage > 0)
            items = items.Skip((page - 1) * (int)itemsPerPage).Take((int)itemsPerPage);

        return asNoTracking ? await items.AsNoTracking().ToArrayAsync() :
                              await items.ToArrayAsync();
    }

    public async Task AddAsync(TEntity entity) => await DbSet.AddAsync(entity);

    public void Delete(TEntity entity) => DbSet.Remove(entity);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression) => DbSet.AnyAsync(expression);

    public async Task<int> SaveAsync() => await dbContext.SaveChangesAsync();
}
