namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Interfaces.Repositories;
using PersonalFinancialManager.Infrastructure.Data;

public class AccountRepository(AppDbContext dbContext) : RepositoryBase<Account>(dbContext), IAccountRepository
{

}
