namespace PersonalFinancialManager.Application.Services;

using Microsoft.AspNetCore.Identity;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using PersonalFinancialManager.Core.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

public class AccountService(IAccountRepository accountRepository, UserManager<AppUser> userManager) : IAccountService
{
    public async Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO)
    {
        var appUser = await userManager.FindByIdAsync(userId);

        if (appUser == null) 
        {
            // TODO: return proper error
            return new ServiceResult<AccountDTO>
            {
                Success = false,
                Errors = []
            };
        }

        Account entity = new()
        {
            Name = createAccountDTO.Name,
            Currency = createAccountDTO.Currency,
            AccountType = (AccountType)Enum.Parse(typeof(AccountType), createAccountDTO.AccountType),
            Description = createAccountDTO.Description,
            Total = createAccountDTO.Total ?? 0.0m,
            CreationDate = DateTime.Now
        };

        await accountRepository.AddAsync(entity);
        appUser.Accounts.Add(entity);

        _ = await accountRepository.SaveAsync();

        ServiceResult<AccountDTO> result = new ()
        {
            Success = true,
            Data = new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Currency = entity.Currency,
                AccountType = entity.AccountType.ToString(),
                Description = entity.Description,
                Total = entity.Total,
                CreationDate = entity.CreationDate.ToString("dd/MM/yyyy")
            }
        };

        return result;
    }

    public Task<ServiceResult> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<AccountDTO>> GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> UpdateAsync(UpdateAccountDTO updateAccountDTO)
    {
        throw new NotImplementedException();
    }
}
