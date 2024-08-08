namespace PersonalFinancialManager.Application.Services;

using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class AccountService(IAccountRepository accountRepository) : IAccountService
{
    public async Task<ServiceResult<AccountDTO>> CreateAsync(string userId, CreateAccountDTO createAccountDTO)
    {
        if (await accountRepository.AnyAsync(e => e.AppUserId.ToString() == userId && e.Name == createAccountDTO.Name))
        {
            return new ServiceResult<AccountDTO> { Success = false, Errors = new() { { "DuplicateName", [$"Name '{createAccountDTO.Name}' already exists."] } } };
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

    public async Task<ServiceResult<QueryResponse<AccountDTO>>> GetAllAsync(QueryModel queryModel, string userId)
    {
        Expression<Func<Account, bool>>? filter = account => account.AppUserId.ToString() == userId;

        if (!string.IsNullOrWhiteSpace(queryModel.Search))
        {
            filter = account => account.AppUserId.ToString() == userId &&
                                account.Name.Contains(queryModel.Search);

            // No need for this for now!
            //ParameterExpression param = filter.Parameters[0];
            //Expression<Func<Account, bool>> searchFilter = account => account.Name.Contains(queryModel.Search);
            //Expression body = Expression.AndAlso(filter.Body, Expression.Invoke(searchFilter, param));

            //filter = Expression.Lambda<Func<Account, bool>>(body, param);
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

        return new()
        {
            Success = true,
            Data = new QueryResponse<AccountDTO>
            {
                Search = queryModel.Search,
                ItemsCount = queryResult.ItemsCount,
                CurrentPage = queryModel.Page ?? 1,
                ItemsPerPage = queryModel.ItemsPerPage,
                Order = order,
                OrderBy = queryModel.OrderBy,
                Items = queryResult.Items.Select(i => new AccountDTO
                {
                    Id = i.Id,
                    Name = i.Name,
                    Currency = i.Currency,
                    AccountType = i.AccountType.ToString(),
                    Description = i.Description,
                    Total = i.Total,
                    CreationDate = i.CreationDate.ToString("dd/MM/yyyy")
                })
            }
        };
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
            return new() { Success = false, Errors = new() { { "DuplicateName", [$"Name '{updateAccountDTO.Name}' already exists."] } } };
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
