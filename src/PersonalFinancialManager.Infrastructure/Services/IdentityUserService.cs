namespace PersonalFinancialManager.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.Core.Entities;
using System.Threading.Tasks;

public class IdentityUserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : IUserService
{
    public async Task<ServiceResult> CreateAsync(CreateUserDTO createUserDTO)
    {
        AppUser user = new()
        {
            Email = createUserDTO.Email,
            UserName = createUserDTO.Email
        };

        var identityResult = await userManager.CreateAsync(user, createUserDTO.Password);
        
        ServiceResult result = new()
        {
            Success = identityResult.Succeeded
        };

        if (!identityResult.Succeeded)
        {
            result.Errors = identityResult.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());
        }

        // TODO: Send email verification link

        return result;
    }
}
