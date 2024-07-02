namespace PersonalFinancialManager.Application.Interfaces;

using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.ServiceModels;

public interface IUserService
{
    Task<ServiceResult> CreateAsync(CreateUserDTO createUserDTO);
}
