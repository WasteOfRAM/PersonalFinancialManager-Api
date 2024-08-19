namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using PersonalFinancialManager.Infrastructure.Data;

public class AccountRepository(AppDbContext dbContext) : RepositoryBase<Account>(dbContext), IAccountRepository
{
    public void UpdateAccountTotal(Account account, TransactionType transactionType, decimal amount)
    {
        if (transactionType == TransactionType.Deposit)
        {
            account.Total += amount;
        }
        else
        {
            account.Total -= amount;
        }

        DbSet.Update(account);
    }
}
