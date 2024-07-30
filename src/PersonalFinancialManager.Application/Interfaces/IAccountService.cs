namespace PersonalFinancialManager.Application.Interfaces;

using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.ServiceModels;

public interface IAccountService
{
    Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO);

    Task<ServiceResult<AccountDTO>> GetAsync(Guid id);

    Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel);

    Task<ServiceResult> UpdateAsync(UpdateAccountDTO updateAccountDTO);

    Task<ServiceResult> DeleteAsync(Guid id);
}