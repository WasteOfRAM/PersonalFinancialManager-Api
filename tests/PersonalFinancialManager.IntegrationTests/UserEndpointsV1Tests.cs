﻿namespace PersonalFinancialManager.IntegrationTests;

using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceScope serviceScope = factory.Services.CreateScope();
    private AppDbContext? appDbContext;

    public static IEnumerable<object[]> InvalidUsers =>
    [
        [new CreateUserDTO("user@test.com", ApplicationValidPassword)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordOne)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordTwo)],
        [new CreateUserDTO("unique@test.com", ApplicationInvalidPasswordThree)]
    ];

    public async Task DisposeAsync()
    {
        await appDbContext!.Database.EnsureDeletedAsync();
        serviceScope.Dispose();
    }

    public async Task InitializeAsync()
    {
        appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext!.Database.MigrateAsync();

        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            Email = "user@test.com",
            UserName = "test@test.com"
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
}