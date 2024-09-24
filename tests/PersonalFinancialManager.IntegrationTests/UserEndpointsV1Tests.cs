namespace PersonalFinancialManager.IntegrationTests;

using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Data;
using System.Net;
using System.Net.Http.Json;

using static PersonalFinancialManager.IntegrationTests.Constants.EndpointsV1;
using static PersonalFinancialManager.IntegrationTests.Constants.Passwords;

public class UserEndpointsV1Tests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly HttpClient httpClient = factory.CreateClient();

    public static IEnumerable<object[]> InvalidUsers =>
    [
        [new CreateUserDTO("user@test.com", ApplicationValidPassword)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordOne)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordTwo)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordThree)]
    ];

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext!.Database.EnsureDeletedAsync();
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext!.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = "user@test.com",
            UserName = "user@test.com"
        };

        await userManager.CreateAsync(user, ApplicationValidPassword);
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
    [MemberData(nameof(InvalidUsers))]
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

        var loginDto = new LoginDTO("user@test.com", ApplicationValidPassword);

        // Act
        var response = await httpClient.PostAsJsonAsync(UserEndpoints.Login, loginDto);

        var jsonResponse = await response.Content.ReadFromJsonAsync<AccessTokenDTO>();

        // Assert
        using var scope = factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await appDbContext!.Users.FirstOrDefaultAsync(u => u.Email == "user@test.com");

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(jsonResponse);
        Assert.Equal(user.RefreshToken, jsonResponse.RefreshToken);
    }
}
