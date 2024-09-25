namespace PersonalFinancialManager.IntegrationTests;

using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using static PersonalFinancialManager.Infrastructure.Constants.InfrastructureValidationMessages;

using static PersonalFinancialManager.IntegrationTests.Constants.EndpointsV1;
using static PersonalFinancialManager.IntegrationTests.Constants.Passwords;

public class UserEndpointsV1Tests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient httpClient = factory.CreateClient();
    private const string seededUserEmail = "user@test.com";

    public static IEnumerable<object[]> InvalidCreateUsers =>
    [
        [new CreateUserDTO(seededUserEmail, ApplicationValidPassword)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordOne)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordTwo)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordThree)]
    ];

    public static IEnumerable<object[]> InvalidLoginUsers =>
    [
        [new LoginDTO(seededUserEmail, ApplicationValidPasswordTwo)],
        [new LoginDTO("nonExistingUser@test.com", ApplicationValidPassword)]
    ];

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext!.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = seededUserEmail,
            UserName = seededUserEmail
        };

        await userManager.CreateAsync(user, ApplicationValidPassword);
    }

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext!.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Register_With_Unique_Email_And_Valid_Password_Returns_StatusCode_Ok()
    {
        // Arrange
        var faker = new Faker<CreateUserDTO>()
            .CustomInstantiator(f => new CreateUserDTO(f.Internet.Email(), ApplicationValidPassword));

        var appUserDTO = faker.Generate();

        // Act
        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Register, appUserDTO);

        // Assert
        using var scope = factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await appDbContext!.Users.FirstOrDefaultAsync(u => u.Email == appUserDTO.Email);

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(InvalidCreateUsers))]
    public async Task Register_With_Existing_Email_Or_Invalid_Password_Returns_StatusCode_BadRequest_With_Errors(CreateUserDTO appUserDTO)
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Register, appUserDTO);

        var problemResult = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemResult?.Errors);
    }

    [Fact]
    public async Task Login_With_Existing_User_And_Correct_Password_Returns_StatusCode_OK_With_Access_And_Refresh_Tokens()
    {
        // Arrange

        var loginDto = new LoginDTO(seededUserEmail, ApplicationValidPassword);

        // Act
        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Login, loginDto);

        var jsonResponse = await response.Content.ReadFromJsonAsync<AccessTokenDTO>();

        // Assert
        using var scope = factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await appDbContext!.Users.FirstOrDefaultAsync(u => u.Email == seededUserEmail);

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(jsonResponse);
        Assert.Equal(user.RefreshToken, jsonResponse.RefreshToken);
    }

    [Theory]
    [MemberData(nameof(InvalidLoginUsers))]
    public async Task Login_With_Invalid_User_Or_Password_Returns_StatusCode_BadRequest_With_Errors(LoginDTO loginDto)
    {
        // Arrange

        // Act
        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Login, loginDto);

        // Assert
        var problemResult = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemResult?.Errors);
        Assert.Collection(problemResult.Errors, error => Assert.Multiple(() =>
        {
            Assert.Equal(ErrorMessages.InvalidLogin.Code, error.Key);
            Assert.Equal(ErrorMessages.InvalidLogin.Description, error.Value.First());
        }));
    }

    [Fact]
    public async Task Refresh_With_Expired_Access_Token_And_Valid_Refresh_Token_Returns_StatusCode_OK_With_New_Access_And_Refresh_Tokens()
    {
        // Arrange
        string testUserEmail = "refreshTokenTest@test.com";

        using var scope = factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = testUserEmail,
            UserName = testUserEmail
        };

        await userManager.CreateAsync(user, ApplicationValidPassword);

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Overriding access token expiration to create a user with expired access token
        configuration["Jwt:TokenExpirationInMinutes"] = "0,0001";

        var loginDto = new LoginDTO(testUserEmail, ApplicationValidPassword);
        var loginResponse = await httpClient.PostAsJsonAsync(UserEndpoints.Login, loginDto);
        var accessTokens = await loginResponse.Content.ReadFromJsonAsync<AccessTokenDTO>();

        var refreshTokenDTO = new RefreshTokenDTO(accessTokens!.RefreshToken);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        configuration["Jwt:TokenExpirationInMinutes"] = "2880";

        // Act

        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Refresh, refreshTokenDTO);
        var refreshedTokens = await response.Content.ReadFromJsonAsync<AccessTokenDTO>();

        using var dbScope = factory.Services.CreateScope();
        var appDbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userEntity = await appDbContext!.Users.FirstOrDefaultAsync(u => u.Email == testUserEmail);

        // Assert

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(refreshedTokens);
        Assert.Equal(userEntity?.RefreshToken, refreshedTokens.RefreshToken);
    }

    [Fact]
    public async Task Refresh_With_Expired_Access_Token_And_Refresh_Token_Returns_StatusCode_Unauthorized()
    {
        // Arrange
        string testUserEmail = "refreshTokenTest@test.com";

        using var scope = factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = testUserEmail,
            UserName = testUserEmail
        };

        await userManager.CreateAsync(user, ApplicationValidPassword);

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Overriding access and refresh tokens expiration to create a user with expired tokens
        configuration["Jwt:TokenExpirationInMinutes"] = "0,0001";
        configuration["Jwt:RefreshTokenExpirationInMinutes"] = "0,0001";

        var loginDto = new LoginDTO(testUserEmail, ApplicationValidPassword);
        var loginResponse = await httpClient.PostAsJsonAsync(UserEndpoints.Login, loginDto);
        var accessTokens = await loginResponse.Content.ReadFromJsonAsync<AccessTokenDTO>();

        var refreshTokenDTO = new RefreshTokenDTO(accessTokens!.RefreshToken);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokens.AccessToken);

        configuration["Jwt:TokenExpirationInMinutes"] = "2880";
        configuration["Jwt:RefreshTokenExpirationInMinutes"] = "5760";

        // Act

        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Refresh, refreshTokenDTO);

        // Assert

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
