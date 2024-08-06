namespace PersonalFinancialManager.Application.Services;

using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using PersonalFinancialManager.Core.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

public class AccountService(IAccountRepository accountRepository) : IAccountService
{
    public async Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO)
    {
        if (await accountRepository.AnyAsync(e => e.AppUserId.ToString() == userId && e.Name == createAccountDTO.Name))
        {
            return new ServiceResult<AccountDTO> { Success = false, Errors = new() { { "DuplicateName", [$"Name '{createAccountDTO.Name}' is already exists."] } } };
        }

        Account entity = new()
        {
            Name = createAccountDTO.Name,
            Currency = createAccountDTO.Currency,
            AccountType = (AccountType)Enum.Parse(typeof(AccountType), createAccountDTO.AccountType),
            Description = createAccountDTO.Description,
            Total = createAccountDTO.Total ?? 0.0m,
            CreationDate = DateTime.Now,
            AppUserId = new Guid(userId)
        };

        await accountRepository.AddAsync(entity);

        _ = await accountRepository.SaveAsync();

        ServiceResult<AccountDTO> result = new()
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

    public Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<AccountDTO>> GetAsync(Guid id, string userId)
    {
        Account? entity = await accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == id);

        if (entity == null)
        {
            return new() { Success = false, Errors = new() { { "NotFound", ["Recourse with given id not found."] } } };
        }

        ServiceResult<AccountDTO> result = new()
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
            return new() { Success = false, Errors = new() { { "DuplicateName", [$"Name '{updateAccountDTO.Name}' is already exists."] } } };
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
}
