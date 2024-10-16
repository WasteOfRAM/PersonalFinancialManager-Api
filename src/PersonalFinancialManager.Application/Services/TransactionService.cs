namespace PersonalFinancialManager.Application.Services;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System;
using System.Globalization;
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

        var accountTotalValidation = ValidateAccountTotalRange(account.Total, createTransactionDTO.Amount, createTransactionDTO.TransactionType);
        if (accountTotalValidation is not null)
        {
            return accountTotalValidation;
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
        accountRepository.UpdateAccountTotal(account, entity.Amount, entity.TransactionType);

        _ = await transactionRepository.SaveAsync();

        ServiceResult<TransactionDTO> result = new()
        {
            Success = true,
            Data = new TransactionDTO
            (
                entity.Id,
                entity.TransactionType.ToString(),
                entity.Amount,
                entity.CreationDate.ToString(DateTimeStringFormat),
                entity.Description,
                account.Id
            )
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

        if (await transactionRepository.AnyAsync(t => t.CreationDate > transaction.CreationDate))
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { ErrorMessages.ForbiddenTransactionDeletion.Code, [ErrorMessages.ForbiddenTransactionDeletion.Description] } } };
        }

        var transactionType = transaction.TransactionType == TransactionType.Deposit ? TransactionType.Withdraw : TransactionType.Deposit;

        accountRepository.UpdateAccountTotal(transaction.Account!, transaction.Amount, transactionType);

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
                                    transaction.Description != null && transaction.Description.Contains(queryModel.Search);
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
            (
                queryModel.Search,
                transactions.ItemsCount,
                queryModel.Page ?? 1,
                queryModel.ItemsPerPage,
                queryModel.OrderBy ?? "CreationDate",
                queryModel.Order ?? "DESC",
                transactions.Items.Select(transaction => new TransactionDTO
                (
                    transaction.Id,
                    transaction.TransactionType.ToString(),
                    transaction.Amount,
                    transaction.CreationDate.ToString(DateTimeStringFormat),
                    transaction.Description,
                    transaction.AccountId
                ))
            )
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
            (
                transaction.Id,
                transaction.TransactionType.ToString(),
                transaction.Amount,
                transaction.CreationDate.ToString(DateTimeStringFormat),
                transaction.Description,
                transaction.AccountId
            )
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

        if (await transactionRepository.AnyAsync(t => t.CreationDate > transaction.CreationDate))
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { ErrorMessages.ForbiddenTransactionEdit.Code, [ErrorMessages.ForbiddenTransactionEdit.Description] } } };
        }

        // Reverting the account total to before the transaction was created. (Without updating the DB)
        var transactionType = transaction.TransactionType == TransactionType.Deposit ? TransactionType.Withdraw : TransactionType.Deposit;
        accountRepository.UpdateAccountTotal(transaction.Account!, transaction.Amount, transactionType);

        var accountTotalValidation = ValidateAccountTotalRange(transaction.Account!.Total, updateTransactionDTO.Amount, updateTransactionDTO.TransactionType);
        if (accountTotalValidation is not null)
        {
            return accountTotalValidation;
        }

        // Updating the transaction.
        transaction.TransactionType = (TransactionType)Enum.Parse(typeof(TransactionType), updateTransactionDTO.TransactionType);
        transaction.Amount = updateTransactionDTO.Amount;
        transaction.Description = updateTransactionDTO.Description;
        transactionRepository.Update(transaction);

        // Updating the account with the updated transaction.
        accountRepository.UpdateAccountTotal(transaction.Account!, transaction.Amount, transaction.TransactionType);

        _ = await transactionRepository.SaveAsync();

        ServiceResult<TransactionDTO> result = new()
        {
            Success = true,
            Data = new TransactionDTO
            (
                transaction.Id,
                transaction.TransactionType.ToString(),
                transaction.Amount,
                transaction.CreationDate.ToString(DateTimeStringFormat),
                transaction.Description,
                transaction.AccountId
            )
        };

        return result;
    }

    private static ServiceResult<TransactionDTO>? ValidateAccountTotalRange(decimal accountCurrentTotal, decimal transactionAmount, string transactionType)
    {
        if (transactionType == nameof(TransactionType.Deposit) && accountCurrentTotal + transactionAmount > decimal.Parse(DecimalRangeMaximumValue, CultureInfo.InvariantCulture))
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { ErrorMessages.AccountTotalMaxValue.Code, [string.Format(ErrorMessages.AccountTotalMaxValue.Description, DecimalRangeMaximumValue)] } } };
        }

        if (transactionType == nameof(TransactionType.Withdraw) && accountCurrentTotal - transactionAmount < decimal.Parse(DecimalRangeMinimumValue, CultureInfo.InvariantCulture))
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { ErrorMessages.AccountTotalMinValue.Code, [ErrorMessages.AccountTotalMinValue.Description] } } };
        }

        return null;
    }
}