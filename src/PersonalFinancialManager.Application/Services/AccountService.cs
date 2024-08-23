namespace PersonalFinancialManager.Application.Services;

using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;
using static PersonalFinancialManager.Application.Constants.ApplicationValidationMessages;

public class AccountService(IAccountRepository accountRepository, ITransactionRepository transactionRepository) : IAccountService
{
    public async Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO)
    {
        if (await accountRepository.AnyAsync(e => e.AppUserId.ToString() == userId && e.Name == createAccountDTO.Name))
        {
            return new ServiceResult<AccountDTO> { Success = false, Errors = new() { { ErrorMessages.DuplicateName.Code, [string.Format(ErrorMessages.DuplicateName.Description, createAccountDTO.Name)] } } };
        }

        Account accountEntity = new()
        {
            Name = createAccountDTO.Name,
            Currency = createAccountDTO.Currency,
            AccountType = (AccountType)Enum.Parse(typeof(AccountType), createAccountDTO.AccountType),
            Description = createAccountDTO.Description,
            Total = createAccountDTO.Total ?? 0.0m,
            CreationDate = DateTime.Now,
            AppUserId = new Guid(userId)
        };

        await accountRepository.AddAsync(accountEntity);

        if (createAccountDTO.Total is not null && createAccountDTO.Total > 0.0m)
        {
            Transaction transaction = new()
            {
                AccountId = accountEntity.Id,
                TransactionType = TransactionType.Deposit,
                Amount = accountEntity.Total,
                Description = "Automated transaction on account creation.",
                CreationDate = accountEntity.CreationDate
            };

            await transactionRepository.AddAsync(transaction);
        }

        _ = await accountRepository.SaveAsync();

        ServiceResult<AccountDTO> result = new()
        {
            Success = true,
            Data = new AccountDTO
            (
                accountEntity.Id,
                accountEntity.Name,
                accountEntity.Currency,
                accountEntity.AccountType.ToString(),
                accountEntity.CreationDate.ToString(DateTimeStringFormat),
                accountEntity.Total,
                accountEntity.Description
            )
        };

        return result;
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, string userId)
    {
        Account? entity = await accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == id);

        if (entity == null)
        {
            return new() { Success = false };
        }

        accountRepository.Delete(entity);
        await accountRepository.SaveAsync();

        return new() { Success = true };
    }

    public async Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel, string userId)
    {
        Expression<Func<Account, bool>>? filter = account => account.AppUserId.ToString() == userId;

        if (!string.IsNullOrWhiteSpace(queryModel.Search))
        {
            filter = account => account.AppUserId.ToString() == userId &&
                                account.Name.Contains(queryModel.Search);
        }

        var queryResult = await accountRepository.GetAllAsync(filter,
            order: queryModel.Order,
            orderBy: queryModel.OrderBy,
            page: queryModel.Page ?? 1,
            itemsPerPage: queryModel.ItemsPerPage);

        string order = queryModel.Order?.ToUpper() switch
        {
            null => "ASC",
            "ASC" => "ASC",
            "DESC" => "DESC",
            _ => "ASC"
        };

        ServiceResult<QueryResponse<AccountDTO>> result = new()
        {
            Success = true,
            Data = new QueryResponse<AccountDTO>
            (
                queryModel.Search,
                queryResult.ItemsCount,
                queryModel.Page ?? 1,
                queryModel.ItemsPerPage,
                queryModel.OrderBy,
                order,
                queryResult.Items.Select(account => new AccountDTO
                (
                    account.Id,
                    account.Name,
                    account.Currency,
                    account.AccountType.ToString(),
                    account.CreationDate.ToString(DateTimeStringFormat),
                    account.Total,
                    account.Description
                ))
            )
        };

        return result;
    }

    public async Task<ServiceResult<AccountDTO>> GetAsync(Guid id, string userId)
    {
        Account? entity = await accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == id);

        if (entity == null)
        {
            return new() { Success = false };
        }

        ServiceResult<AccountDTO> result = new()
        {
            Success = true,
            Data = new AccountDTO
            (
                entity.Id,
                entity.Name,
                entity.Currency,
                entity.AccountType.ToString(),
                entity.CreationDate.ToString(DateTimeStringFormat),
                entity.Total,
                entity.Description
            )
        };

        return result;
    }

    public async Task<ServiceResult<AccountWithTransactionsDTO>> GetWithTransactionsAsync(Guid id, QueryModel transactionsQuery, string userId)
    {
        Account? account = await accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == id);

        if (account == null)
        {
            return new() { Success = false };
        }

        Expression<Func<Transaction, bool>>? filter = transaction => transaction.AccountId == id;

        if (!string.IsNullOrWhiteSpace(transactionsQuery.Search))
        {
            filter = transaction => transaction.AccountId == id &&
                                    transaction.Description != null ? transaction.Description.Contains(transactionsQuery.Search) : false;
        }

        var queryResult = await transactionRepository.GetAllAsync(filter,
            order: transactionsQuery.Order ?? "DESC",
            orderBy: transactionsQuery.OrderBy ?? "CreationDate",
            itemsPerPage: transactionsQuery.ItemsPerPage,
            page: transactionsQuery.Page ?? 1);

        AccountWithTransactionsDTO accountWithTransactionsDTO = new
        (
            account.Id,
            account.Name,
            account.Currency,
            account.AccountType.ToString(),
            account.CreationDate.ToString(DateTimeStringFormat),
            account.Total,
            account.Description,
            new QueryResponse<TransactionDTO>
            (
                transactionsQuery.Search,
                queryResult.ItemsCount,
                transactionsQuery.Page ?? 1,
                transactionsQuery.ItemsPerPage,
                transactionsQuery.OrderBy ?? "CreationDate",
                transactionsQuery.Order ?? "DESC",
                queryResult.Items.Select(transaction => new TransactionDTO
                (
                    transaction.Id,
                    transaction.TransactionType.ToString(),
                    transaction.Amount,
                    transaction.CreationDate.ToString(DateTimeStringFormat),
                    transaction.Description,
                    transaction.AccountId
                ))
            )
        );

        ServiceResult<AccountWithTransactionsDTO> result = new()
        {
            Success = true,
            Data = accountWithTransactionsDTO
        };

        return result;
    }

    public async Task<ServiceResult<AccountDTO>> UpdateAsync(UpdateAccountDTO updateAccountDTO, string userId)
    {
        Guid accountId = Guid.Parse(updateAccountDTO.Id);

        Account? entity = await accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == accountId);

        if (entity == null)
        {
            return new() { Success = false };
        }

        if (await accountRepository.AnyAsync(e => e.AppUserId.ToString() == userId && e.Name == updateAccountDTO.Name))
        {
            return new() { Success = false, Errors = new() { { ErrorMessages.DuplicateName.Code, [string.Format(ErrorMessages.DuplicateName.Description, updateAccountDTO.Name)] } } };
        }

        entity.Name = updateAccountDTO.Name;
        entity.Currency = updateAccountDTO.Currency;
        entity.AccountType = (AccountType)Enum.Parse(typeof(AccountType), updateAccountDTO.AccountType);
        entity.Description = updateAccountDTO.Description;

        accountRepository.Update(entity);
        await accountRepository.SaveAsync();

        ServiceResult<AccountDTO> result = new()
        {
            Success = true,
            Data = new AccountDTO
            (
                entity.Id,
                entity.Name,
                entity.Currency,
                entity.AccountType.ToString(),
                entity.CreationDate.ToString(DateTimeStringFormat),
                entity.Total,
                entity.Description
            )
        };

        return result;
    }
}
