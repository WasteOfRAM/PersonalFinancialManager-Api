namespace PersonalFinancialManager.Application.Interfaces.Services;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;

public interface ITransactionService
{
    Task<ServiceResult<TransactionDTO>> CreateAsync(CreateTransactionDTO createTransactionDTO, string userId);

    Task<ServiceResult<TransactionDTO>> GetAsync(Guid id, string userId);

    Task<ServiceResult<QueryResponse<TransactionDTO>>> GetAllAsync(QueryModel queryModel, string userId);

    Task<ServiceResult<TransactionDTO>> UpdateAsync(UpdateTransactionDTO updateTransactionDTO, string userId);

    Task<ServiceResult> DeleteAsync(Guid id, string userId);
}
