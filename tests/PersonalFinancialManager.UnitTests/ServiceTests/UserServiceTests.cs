namespace PersonalFinancialManager.UnitTests.ServiceTests;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Services;

public class UserServiceTests
{
    private readonly UserManager<AppUser> userManager;
    private readonly SignInManager<AppUser> signInManager;

    private readonly UserService userService;

    public UserServiceTests()
    {

        var userStore = Substitute.For<IUserStore<AppUser>>();
        userManager = Substitute.For<UserManager<AppUser>>(userStore, null, null, null, null, null, null, null, null);

        // SignInManager dependencies
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<AppUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var logger = Substitute.For<ILogger<SignInManager<AppUser>>>();
        var schemes = Substitute.For<IAuthenticationSchemeProvider>();
        var confirmation = Substitute.For<IUserConfirmation<AppUser>>();

        // SignInManager mock
        signInManager = Substitute.For<SignInManager<AppUser>>(
            userManager,
            contextAccessor,
            claimsFactory,
            options,
            logger,
            schemes,
            confirmation
        );

        var tokenService = Substitute.For<ITokenService>();
        var configuration = Substitute.For<IConfiguration>();

        userService = new UserService(userManager, signInManager, tokenService, configuration);
    }

    [Fact]
    public async Task Creating_User_With_Valid_Email_And_Password_Returns_ResultSuccess_True()
    {
        userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

        var userDTO = new CreateUserDTO("test@test.com", "Password1!12121212");

        var result = await userService.CreateAsync(userDTO);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task Creating_User_With_Invalid_Data_Returns_ResultSuccess_False_And_ResultErrors_Dictionary()
    {
        var identityResult = new IdentityResult { Errors = { } };

        userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(Task.FromResult(identityResult));

        var userDTO = new CreateUserDTO("test@test.com", "12345");

        var result = await userService.CreateAsync(userDTO);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        });
    }
}
