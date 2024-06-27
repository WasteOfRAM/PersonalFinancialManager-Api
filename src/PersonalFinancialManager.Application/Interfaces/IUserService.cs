namespace PersonalFinancialManager.Application.Interfaces;

using PersonalFinancialManager.Application.DTOs.User;

public interface IUserService
{
    Task<UserDTO?> CreateAsync(CreateUserDTO createUserDTO);
}
