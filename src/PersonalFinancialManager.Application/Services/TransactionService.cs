namespace PersonalFinancialManager.Application.Services;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System;
using System.Threading.Tasks;

public class TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository) : ITransactionService
{
    public async Task<ServiceResult<TransactionDTO>> CreateAsync(CreateTransactionDTO createTransactionDTO, string userId)
    {
        Account? account = await accountRepository.GetAsync(a => a.Id.ToString() == createTransactionDTO.AccountId && a.AppUserId.ToString() == userId);

        if (account is null)
        {
            return new ServiceResult<TransactionDTO> { Success = false, Errors = new() { { "AccountId", ["Account with given Id does not exist."] } } };
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
                CreationDate = entity.CreationDate.ToString("dd/MM/yyyy")
            }
        };

        return result;
    }

    public Task<ServiceResult> DeleteAsync(Guid id, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<QueryResponse<TransactionDTO>>> GetAllAsync(QueryModel queryModel, string userId)
    {   
        throw new NotImplementedException();
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
                CreationDate = transaction.CreationDate.ToString("dd/MM/yyyy")
            }
        };

        return result;
    }

    public Task<ServiceResult<TransactionDTO>> UpdateAsync(UpdateTransactionDTO updateTransactionDTO, string userId)
    {
        throw new NotImplementedException();
    }
}
