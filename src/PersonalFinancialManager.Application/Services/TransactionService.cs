namespace PersonalFinancialManager.Application.Services;

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

public class TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository) : ITransactionService
{
    public async Task<ServiceResult<TransactionDTO>> CreateAsync(CreateTransactionDTO createTransactionDTO, string userId)
    {
        Account? account = await accountRepository.GetAsync(a => a.Id.ToString() == createTransactionDTO.AccountId && a.AppUserId.ToString() == userId);

        if (account is null)
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { ErrorMessages.AccountId.Code, [ErrorMessages.AccountId.Description] } } };
        }

        Transaction entity = new()
        {
            AccountId = account.Id,
            TransactionType = (TransactionType)Enum.Parse(typeof(TransactionType), createTransactionDTO.TransactionType),
            Amount = createTransactionDTO.Amount,
            CreationDate = DateTime.Now,
            Description = createTransactionDTO.Description
        };

        await transactionRepository.AddAsync(entity);
        accountRepository.UpdateAccountTotal(account, entity.TransactionType, entity.Amount);

        _ = await transactionRepository.SaveAsync();

        ServiceResult<TransactionDTO> result = new()
        {
            Success = true,
            Data = new TransactionDTO
            {
                Id = entity.Id,
                TransactionType = entity.TransactionType.ToString(),
                Amount = entity.Amount,
                Description = entity.Description,
                AccountId = account.Id,
                CreationDate = entity.CreationDate.ToString(DateTimeStringFormat)
            }
        };

        return result;
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, string userId)
    {
        var transaction = await transactionRepository.GetAsync(t => t.Id == id && t.Account!.AppUserId.ToString() == userId, includeProperty: "Account");

        if (transaction == null)
        {
            return new() { Success = false };
        }

        var transactionType = transaction.TransactionType == TransactionType.Deposit ? TransactionType.Withdraw : TransactionType.Deposit;

        accountRepository.UpdateAccountTotal(transaction.Account!, transactionType, transaction.Amount);

        transactionRepository.Delete(transaction);

        _ = await transactionRepository.SaveAsync();

        return new() { Success = true };
    }

    public async Task<ServiceResult<QueryResponse<TransactionDTO>>> GetAllAsync(QueryModel queryModel, string userId)
    {
        Expression<Func<Transaction, bool>>? filter = transaction => transaction.Account!.AppUserId.ToString() == userId;

        if (!string.IsNullOrWhiteSpace(queryModel.Search))
        {
            filter = transaction => transaction.Account!.AppUserId.ToString() == userId &&
                                    transaction.Description != null ? transaction.Description.Contains(queryModel.Search) : false;
        }

        var transactions = await transactionRepository.GetAllAsync(filter,
            order: queryModel.Order ?? "DESC",
            orderBy: queryModel.OrderBy ?? "CreationDate",
            itemsPerPage: queryModel.ItemsPerPage,
            page: queryModel.Page ?? 1,
            includeProperty: "Account");

        ServiceResult<QueryResponse<TransactionDTO>> result = new()
        {
            Success = true,
            Data = new QueryResponse<TransactionDTO>
            {
                Items = transactions.Items.Select(t => new TransactionDTO
                {
                    AccountId = t.AccountId,
                    TransactionType = t.TransactionType.ToString(),
                    Amount = t.Amount,
                    Description = t.Description,
                    CreationDate = t.CreationDate.ToString(DateTimeStringFormat),
                    Id = t.Id
                }),
                ItemsCount = transactions.ItemsCount,
                CurrentPage = queryModel.Page ?? 1,
                Search = queryModel.Search,
                Order = queryModel.Order ?? "DESC",
                OrderBy = queryModel.OrderBy ?? "CreationDate",
                ItemsPerPage = queryModel.ItemsPerPage
            }
        };

        return result;
    }

    public async Task<ServiceResult<TransactionDTO>> GetAsync(Guid id, string userId)
    {
        Transaction? transaction = await transactionRepository.GetByIdAsync(id);

        if (transaction == null)
        {
            return new ServiceResult<TransactionDTO> { Success = false };
        }

        Account? account = await accountRepository.GetAsync(a => a.AppUserId.ToString() == userId && a.Id == transaction.AccountId);

        if (account == null)
        {
            return new ServiceResult<TransactionDTO> { Success = false };
        }

        ServiceResult<TransactionDTO> result = new()
        {
            Success = true,
            Data = new TransactionDTO
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType.ToString(),
                Amount = transaction.Amount,
                Description = transaction.Description,
                AccountId = transaction.AccountId,
                CreationDate = transaction.CreationDate.ToString(DateTimeStringFormat)
            }
        };

        return result;
    }

    public async Task<ServiceResult<TransactionDTO>> UpdateAsync(UpdateTransactionDTO updateTransactionDTO, string userId)
    {
        var transaction = await transactionRepository.GetAsync(t => t.Id.ToString() == updateTransactionDTO.Id &&
                t.Account!.AppUserId.ToString() == userId &&
                t.AccountId.ToString() == updateTransactionDTO.AccountId,
                includeProperty: "Account");

        if (transaction == null)
        {
            return new() { Success = false };
        }

        // Reverting the account total to before the transaction was created.
        var transactionType = transaction.TransactionType == TransactionType.Deposit ? TransactionType.Withdraw : TransactionType.Deposit;
        accountRepository.UpdateAccountTotal(transaction.Account!, transactionType, transaction.Amount);

        // Updating the transaction.
        transaction.TransactionType = (TransactionType)Enum.Parse(typeof(TransactionType), updateTransactionDTO.TransactionType);
        transaction.Amount = updateTransactionDTO.Amount;
        transaction.Description = updateTransactionDTO.Description;
        transactionRepository.Update(transaction);

        // Updating the account with the updated transaction.
        accountRepository.UpdateAccountTotal(transaction.Account!, transaction.TransactionType, transaction.Amount);

        _ = await transactionRepository.SaveAsync();

        ServiceResult<TransactionDTO> result = new()
        {
            Success = true,
            Data = new TransactionDTO()
            {
                Id = transaction.Id,
                TransactionType = transaction.TransactionType.ToString(),
                Amount = transaction.Amount,
                Description = transaction.Description,
                AccountId = transaction.AccountId,
                CreationDate = transaction.CreationDate.ToString(DateTimeStringFormat)
            }
        };

        return result;
    }
}