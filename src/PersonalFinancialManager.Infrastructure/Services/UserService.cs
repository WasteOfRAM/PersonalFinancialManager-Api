namespace PersonalFinancialManager.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using System.Threading.Tasks;

using static PersonalFinancialManager.Infrastructure.Constants.InfrastructureValidationMessages;

public class UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IConfiguration configuration) : IUserService
{
    public async Task<ServiceResult> CreateAsync(CreateUserDTO createUserDTO)
    {
        AppUser user = new()
        {
            Email = createUserDTO.Email,
            UserName = createUserDTO.Email
        };

        var identityResult = await userManager.CreateAsync(user, createUserDTO.Password);

        ServiceResult result = new() { Success = identityResult.Succeeded };

        if (!identityResult.Succeeded)
        {
            result.Errors = identityResult.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
        }

        // TODO: Send email verification link

        return result;
    }

    public async Task<ServiceResult<AccessTokenDTO>> LoginAsync(LoginDTO loginDTO)
    {
        var result = new ServiceResult<AccessTokenDTO>();

        var user = await userManager.FindByEmailAsync(loginDTO.Email);
        SignInResult? signInResult = null;

        if (user != null)
        {
            signInResult = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (signInResult.Succeeded)
            {
                var principal = await signInManager.CreateUserPrincipalAsync(user);

                AccessTokenDTO token = new()
                {
                    AccessToken = tokenService.GenerateAccessToken(principal.Claims),
                    RefreshToken = tokenService.GenerateRefreshToken()
                };

                user.RefreshToken = token.RefreshToken;

                double tokenExpiration = double.Parse(configuration["Jwt:RefreshTokenExpirationInMinutes"]!);
                user.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(tokenExpiration);

                await userManager.UpdateAsync(user);

                result.Success = true;
                result.Data = token;
            }
        }

        if (user == null || !signInResult!.Succeeded)
        {
            result.Success = false;
            result.Errors = new Dictionary<string, string[]> { { ErrorMessages.InvalidLogin.Code, [ErrorMessages.InvalidLogin.Description] } };
        }

        return result;
    }

    public async Task<ServiceResult<AccessTokenDTO>> TokenRefresh(string userId, string refreshToken)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiration > DateTime.Now)
        {
            return new ServiceResult<AccessTokenDTO>
            {
                Success = false
            };
        }

        var principal = await signInManager.CreateUserPrincipalAsync(user);

        AccessTokenDTO token = new()
        {
            AccessToken = tokenService.GenerateAccessToken(principal.Claims),
            RefreshToken = tokenService.GenerateRefreshToken()
        };

        user.RefreshToken = token.RefreshToken;

        double tokenExpiration = double.Parse(configuration["Jwt:RefreshTokenExpirationInMinutes"]!);
        user.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(tokenExpiration);

        await userManager.UpdateAsync(user);

        var result = new ServiceResult<AccessTokenDTO>
        {
            Success = true,
            Data = token
        };

        return result;
    }
}
