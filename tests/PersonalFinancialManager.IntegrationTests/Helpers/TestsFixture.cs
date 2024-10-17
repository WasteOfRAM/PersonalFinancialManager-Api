namespace PersonalFinancialManager.IntegrationTests.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Infrastructure.Data;
using System.Net.Http.Json;

using static PersonalFinancialManager.IntegrationTests.Constants.EndpointsV1;
using static PersonalFinancialManager.IntegrationTests.Constants.Commons;


public class TestsFixture
{   
    private readonly CustomWebApplicationFactory<Program> appFactory;
    private readonly string seededUserAccessToken;
    private readonly string seededUserId;

    public CustomWebApplicationFactory<Program> AppFactory { get { return appFactory; } }
    public string SeededUserAccessToken { get { return seededUserAccessToken; } }
    public string SeededUserId {  get { return seededUserId; } }

    public TestsFixture()
    {
        appFactory = new CustomWebApplicationFactory<Program>();

        using HttpClient httpClient = appFactory.CreateClient();

        using var scope = appFactory.Services.CreateScope();

        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        appDbContext!.Database.MigrateAsync().GetAwaiter().GetResult();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = UserEmails.TestUserEmail,
            UserName = UserEmails.TestUserEmail
        };

        userManager.CreateAsync(user, Passwords.ApplicationValidPassword).GetAwaiter().GetResult();

        seededUserId = user.Id.ToString();

        var response = httpClient.PostAsJsonAsync(UserEndpoints.Login, new LoginDTO(UserEmails.TestUserEmail, Passwords.ApplicationValidPassword)).GetAwaiter().GetResult();
        var accessTokenDTO = response.Content.ReadFromJsonAsync<AccessTokenDTO>().GetAwaiter().GetResult();

        seededUserAccessToken = accessTokenDTO!.AccessToken;
    }
}