namespace PersonalFinancialManager.Application.Interfaces;

using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.ServiceModels;

public interface IUserService
{
    Task<ServiceResult> CreateAsync(CreateUserDTO createUserDTO);

    Task<ServiceResult<AccessTokenDTO>> LoginAsync(LoginDTO loginDTO);

    Task<ServiceResult<AccessTokenDTO>> TokenRefresh(string userId, string refreshToken);
}
