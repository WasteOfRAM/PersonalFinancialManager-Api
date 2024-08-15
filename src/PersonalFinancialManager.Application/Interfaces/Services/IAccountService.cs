namespace PersonalFinancialManager.Application.Interfaces.Services;

using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;

public interface IAccountService
{
    Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO);

    Task<ServiceResult<AccountDTO>> GetAsync(Guid id, string userId);

    Task<ServiceResult<AccountWithTransactionsDTO>> GetWithTransactionsAsync(Guid id, QueryModel transactionsQuery, string userId);

    Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel, string userId);

    Task<ServiceResult<AccountDTO>> UpdateAsync(UpdateAccountDTO updateAccountDTO, string userId);

    Task<ServiceResult> DeleteAsync(Guid id, string userId);
}