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

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false)
    {
        return asNoTracking ? await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate) :
                              await DbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, bool asNoTracking = false, int page = 1, int itemsPerPage = 0, string? order = null, string? orderBy = null)
    {
        var elements = predicate is not null ? DbSet.Where(predicate) : DbSet.AsQueryable();

        if (order != null && orderBy != null)
            elements = elements.OrderBy(orderBy, order);

        if (itemsPerPage > 0)
            elements = elements.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);

        return asNoTracking ? await elements.AsNoTracking().ToArrayAsync() :
                              await elements.ToArrayAsync();
    }

    public async Task AddAsync(TEntity entity) => await DbSet.AddAsync(entity);

    public void Delete(TEntity entity) => DbSet.Remove(entity);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public async Task<int> SaveAsync() => await dbContext.SaveChangesAsync();
}
