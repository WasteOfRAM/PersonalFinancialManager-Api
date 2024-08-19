namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Data;

public class TransactionRepository(AppDbContext dbContext) : RepositoryBase<Transaction>(dbContext), ITransactionRepository
{
}
