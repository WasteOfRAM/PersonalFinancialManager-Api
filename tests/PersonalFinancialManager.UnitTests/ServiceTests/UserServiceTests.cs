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
using System.Security.Claims;

public class UserServiceTests
{
    private readonly UserManager<AppUser> userManager;
    private readonly SignInManager<AppUser> signInManager;
    private readonly Fixture fixture;

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
        tokenService.GenerateAccessToken(Arg.Any<IEnumerable<Claim>>())
            .Returns("eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQ3MDQyZWE2LTExNjUtNDNjNS04YmFhLTA4ZGNjNWNhMTAwMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJ0ZXN0QHRlc3QuYmciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ0ZXN0QHRlc3QuYmciLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IkZFUjZWQ04yV1VEN0NYRVBNTVVGQjZQNjVVRVhIUVRFIiwiZXhwIjoxNzI0ODQ3ODg1LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MjI4In0.i7hIzCjYQsirMDGBU-KgR1GFN3rIKE4Hm_9Or_tiU6o");
        tokenService.GenerateRefreshToken()
            .Returns("I64zpHLXJG1YfxRnnZ7G6JQogp+D4VUeZilJpy4Al3FodmxskudlojdL4IZhUFgQLZOZz1tcWbyMmEwr8LlO21OjiF2558Cy8NCiEbzes4QO1qp/XB2iO4vuT3XFV/aLf0pzXjZ3pq+/1O7d1oF6e63mSBpD/ceZuQc3VWOVWqbkcDK9rv39zRmxridsOIuGOrSbAhIoxTwvJyuMg3jw4o8SzKS/L9cBkrx4XEx9AdtoBTJiZM6+/ZYe15ky+KOVElyF63wD5CNOro+lWPZYGqlgLAlWlUAzE/kqtwrkltXr1YFFR4ExiojXODMz5UclO2sJM6VSxMiFSfsCCr9dMQ==");

        var configuration = Substitute.For<IConfiguration>();
        configuration["Jwt:Key"].Returns("erhgrRTGsadfef545erdgdfgr6tDFHGR4653fgdggsdf");
        configuration["Jwt:Issuer"].Returns("testIssuer");
        configuration["Jwt:Audience"].Returns("testAudience");
        configuration["Jwt:TokenExpirationInMinutes"].Returns("10");
        configuration["Jwt:RefreshTokenExpirationInMinutes"].Returns("10");

        userService = new UserService(userManager, signInManager, tokenService, configuration);

        fixture = new Fixture();

        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateAsync_With_Valid_Email_And_Password_Returns_ResultSuccess_True()
    {
        userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

        var userDTO = fixture.Create<CreateUserDTO>();

        var result = await userService.CreateAsync(userDTO);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task CreateAsync_With_Invalid_Data_Returns_ResultSuccess_False_And_ResultErrors_Dictionary()
    {
        var identityResult = new IdentityResult { Errors = { } };

        userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "Fake error", Description = "Fake description" })));

        var userDTO = fixture.Create<CreateUserDTO>();

        var result = await userService.CreateAsync(userDTO);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotEmpty(result.Errors);
        });
    }

    [Fact]
    public async Task LoginAsync_With_Valid_Email_And_Password_Returns_ResultSuccess_True_With_Data_AccessTokens()
    {
        var loginDTO = fixture.Create<LoginDTO>();
        var appUser = fixture.Create<AppUser>();

        userManager.FindByEmailAsync(loginDTO.Email).Returns(appUser);
        signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false).Returns(SignInResult.Success);
        signInManager.CreateUserPrincipalAsync(appUser).Returns(fixture.Create<ClaimsPrincipal>());
        userManager.UpdateAsync(appUser).Returns(IdentityResult.Success);

        var result = await userService.LoginAsync(loginDTO);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
        });

    }

    [Fact]
    public async Task LoginAsync_With_Non_Existing_User_Returns_ResultSuccess_False_With_Error()
    {
        var loginDTO = fixture.Create<LoginDTO>();

        userManager.FindByEmailAsync(loginDTO.Email).ReturnsNull();

        var result = await userService.LoginAsync(loginDTO);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotEmpty(result.Errors);
        });
    }

    [Fact]
    public async Task LoginAsync_With_Existing_User_Wrong_Password_Returns_ResultSuccess_False_With_Error()
    {
        var loginDTO = fixture.Create<LoginDTO>();
        var appUser = fixture.Create<AppUser>();

        userManager.FindByEmailAsync(loginDTO.Email).Returns(appUser);
        signInManager.CheckPasswordSignInAsync(appUser, loginDTO.Password, false).Returns(SignInResult.Failed);

        var result = await userService.LoginAsync(loginDTO);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotEmpty(result.Errors);
        });
    }

    [Fact]
    public async Task TokenRefresh_With_Valid_Data_Returns_ResultSuccess_True_With_Data_AccessTokens()
    {
        var userId = Guid.NewGuid();
        string refreshToken = "I64zpHLXJG1YfxRnnZ7G6JQogp+D4VUeZilJpy4Al3FodmxskudlojdL4IZhUFgQLZOZz1tcWbyMmEwr8LlO21OjiF2558Cy8NCiEbzes4QO1qp/XB2iO4vuT3XFV/aLf0pzXjZ3pq+/1O7d1oF6e63mSBpD/ceZuQc3VWOVWqbkcDK9rv39zRmxridsOIuGOrSbAhIoxTwvJyuMg3jw4o8SzKS/L9cBkrx4XEx9AdtoBTJiZM6+/ZYe15ky+KOVElyF63wD5CNOro+lWPZYGqlgLAlWlUAzE/kqtwrkltXr1YFFR4ExiojXODMz5UclO2sJM6VSxMiFSfsCCr9dMQ==";
        var refreshTokenExpiration = DateTime.Parse("2100-02-02");

        var appUser = fixture
            .Build<AppUser>()
            .With(u => u.Id, userId)
            .With(u => u.RefreshToken, refreshToken)
            .With(u => u.RefreshTokenExpiration, refreshTokenExpiration)
            .Create();

        userManager.FindByIdAsync(Arg.Any<string>()).Returns(appUser);

        signInManager.CreateUserPrincipalAsync(appUser).Returns(fixture.Create<ClaimsPrincipal>());

        userManager.UpdateAsync(appUser).Returns(IdentityResult.Success);

        var result = await userService.TokenRefresh(userId.ToString(), refreshToken);

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task TokenRefresh_With_Invalid_UserId_Returns_ResultSuccess_False_With_Data_Null()
    {
        var userId = Guid.NewGuid();
        string refreshToken = "I64zpHLXJG1YfxRnnZ7G6JQogp+D4VUeZilJpy4Al3FodmxskudlojdL4IZhUFgQLZOZz1tcWbyMmEwr8LlO21OjiF2558Cy8NCiEbzes4QO1qp/XB2iO4vuT3XFV/aLf0pzXjZ3pq+/1O7d1oF6e63mSBpD/ceZuQc3VWOVWqbkcDK9rv39zRmxridsOIuGOrSbAhIoxTwvJyuMg3jw4o8SzKS/L9cBkrx4XEx9AdtoBTJiZM6+/ZYe15ky+KOVElyF63wD5CNOro+lWPZYGqlgLAlWlUAzE/kqtwrkltXr1YFFR4ExiojXODMz5UclO2sJM6VSxMiFSfsCCr9dMQ==";

        userManager.FindByIdAsync(userId.ToString()).ReturnsNull();

        var result = await userService.TokenRefresh(userId.ToString(), refreshToken);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task TokenRefresh_With_Invalid_RefreshToken_Returns_ResultSuccess_False_With_Data_Null()
    {
        var userId = Guid.NewGuid();
        string refreshToken = "I64zpHLXJG1YfxRnnZ7G6JQogp+D4VUeZilJpy4Al3FodmxskudlojdL4IZhUFgQLZOZz1tcWbyMmEwr8LlO21OjiF2558Cy8NCiEbzes4QO1qp/XB2iO4vuT3XFV/aLf0pzXjZ3pq+/1O7d1oF6e63mSBpD/ceZuQc3VWOVWqbkcDK9rv39zRmxridsOIuGOrSbAhIoxTwvJyuMg3jw4o8SzKS/L9cBkrx4XEx9AdtoBTJiZM6+/ZYe15ky+KOVElyF63wD5CNOro+lWPZYGqlgLAlWlUAzE/kqtwrkltXr1YFFR4ExiojXODMz5UclO2sJM6VSxMiFSfsCCr9dMQ==";
        var refreshTokenExpiration = DateTime.Parse("2100-02-02");

        var appUser = fixture
            .Build<AppUser>()
            .With(u => u.Id, userId)
            .With(u => u.RefreshTokenExpiration, refreshTokenExpiration)
            .Create();

        userManager.FindByIdAsync(userId.ToString()).Returns(appUser);

        var result = await userService.TokenRefresh(userId.ToString(), refreshToken);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task TokenRefresh_With_Valid_Expired_RefreshToken_Returns_ResultSuccess_False_With_Data_Null()
    {
        var userId = Guid.NewGuid();
        string refreshToken = "I64zpHLXJG1YfxRnnZ7G6JQogp+D4VUeZilJpy4Al3FodmxskudlojdL4IZhUFgQLZOZz1tcWbyMmEwr8LlO21OjiF2558Cy8NCiEbzes4QO1qp/XB2iO4vuT3XFV/aLf0pzXjZ3pq+/1O7d1oF6e63mSBpD/ceZuQc3VWOVWqbkcDK9rv39zRmxridsOIuGOrSbAhIoxTwvJyuMg3jw4o8SzKS/L9cBkrx4XEx9AdtoBTJiZM6+/ZYe15ky+KOVElyF63wD5CNOro+lWPZYGqlgLAlWlUAzE/kqtwrkltXr1YFFR4ExiojXODMz5UclO2sJM6VSxMiFSfsCCr9dMQ==";
        var refreshTokenExpiration = DateTime.Parse("2024-02-02");

        var appUser = fixture
            .Build<AppUser>()
            .With(u => u.Id, userId)
            .With(u => u.RefreshToken, refreshToken)
            .With(u => u.RefreshTokenExpiration, refreshTokenExpiration)
            .Create();

        userManager.FindByIdAsync(userId.ToString()).Returns(appUser);

        var result = await userService.TokenRefresh(userId.ToString(), refreshToken);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);
        });
    }
}
