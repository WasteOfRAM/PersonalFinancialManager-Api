namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Interfaces.Repositories;
using PersonalFinancialManager.Infrastructure.Data;

public class TransactionRepository(AppDbContext dbContext) : RepositoryBase<Transaction>(dbContext), ITransactionRepository
{

}
