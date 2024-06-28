namespace PersonalFinancialManager.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using PersonalFinancialManager.Application.DTOs.Token;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Core.Entities;
using System.Threading.Tasks;

public class IdentityUserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : IUserService
{
    public async Task<UserDTO?> CreateAsync(CreateUserDTO createUserDTO)
    {
        AppUser user = new()
        {
            Email = createUserDTO.Email,
            UserName = createUserDTO.Email
        };

        var result = await userManager.CreateAsync(user, createUserDTO.Password);

        if (!result.Succeeded) return null;

        // TODO: Send email verification link

        return new UserDTO(Id: user.Id, Email: user.Email);
    }
}
