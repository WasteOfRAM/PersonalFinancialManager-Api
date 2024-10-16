﻿namespace PersonalFinancialManager.Application.Interfaces.Repositories;

using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;

public interface IAccountRepository : IRepositoryBase<Account>
{
    void UpdateAccountTotal(Account account, decimal transactionAmount, TransactionType transactionType);
}
