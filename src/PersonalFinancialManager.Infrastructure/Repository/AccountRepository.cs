namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Data;

public class AccountRepository(AppDbContext dbContext) : RepositoryBase<Account>(dbContext), IAccountRepository
{

}
