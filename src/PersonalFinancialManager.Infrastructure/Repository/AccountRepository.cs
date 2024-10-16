namespace PersonalFinancialManager.Infrastructure.Repository;

using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using PersonalFinancialManager.Infrastructure.Data;

public class AccountRepository(AppDbContext dbContext) : RepositoryBase<Account>(dbContext), IAccountRepository
{
    public void UpdateAccountTotal(Account account, decimal transactionAmount, TransactionType transactionType)
    {
        if (transactionType == TransactionType.Deposit)
        {
            account.Total += transactionAmount;
        }
        else
        {
            account.Total -= transactionAmount;
        }

        DbSet.Update(account);
    }
}
