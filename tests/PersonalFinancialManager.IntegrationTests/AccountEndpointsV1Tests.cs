namespace PersonalFinancialManager.IntegrationTests;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Infrastructure.Data;
using PersonalFinancialManager.IntegrationTests.DataGenerators;
using PersonalFinancialManager.IntegrationTests.Helpers;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;
using static PersonalFinancialManager.Application.Constants.ApplicationValidationMessages;
using static PersonalFinancialManager.IntegrationTests.Constants.EndpointsV1;

[Collection("Tests collection")]
public class AccountEndpointsV1Tests(TestsFixture testsFixture)
{
    private readonly HttpClient httpClient = testsFixture.AppFactory.CreateClient();
    private readonly string testUserAccessToken = testsFixture.SeededUserAccessToken;

    private readonly AccountDataGenerator accountDataGenerator = new(Guid.Parse(testsFixture.SeededUserId));

    [Fact]
    public async Task CreateAccount_With_Valid_Data_Returns_StatusCode_Ok_And_The_Created_Item()
    {
        // Arrange
        var createAccountDtoList = accountDataGenerator.GenerateCreateAccountDTO();
        var createAccountDto = createAccountDtoList[0];

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        // Act
        var response = await httpClient.PostAsJsonAsync(AccountEndpoints.AccountBase, createAccountDto);
        var responseAccountDto = await response.Content.ReadFromJsonAsync<AccountDTO>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseAccountDto);
        Assert.Multiple(() =>
        {
            Assert.Equal(createAccountDto.Name, responseAccountDto.Name);
            Assert.Equal(createAccountDto.Currency, responseAccountDto.Currency);
            Assert.Equal(createAccountDto.AccountType, responseAccountDto.AccountType);
            Assert.Equal(createAccountDto.Total ?? 0.0m, responseAccountDto.Total);
            Assert.Equal(createAccountDto.Description, responseAccountDto.Description);
        });

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var createdEntity = await appDbContext.Accounts.FindAsync(responseAccountDto.Id);

        Assert.NotNull(createdEntity);

        // Cleanup

        appDbContext.Remove(createdEntity);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAccount_With_Invalid_Data_Returns_StatusCode_BadRequest_With_Errors()
    {
        // Arrange
        var createAccountDto = accountDataGenerator.GenerateCreateAccountDTO();
        var invalidCreateAccountDto = createAccountDto[0] with { Name = "This name is to long!!!" };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        // Act
        var response = await httpClient.PostAsJsonAsync(AccountEndpoints.AccountBase, invalidCreateAccountDto);
        var problemResult = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemResult?.Errors);
    }

    [Fact]
    public async Task Get_Single_Account_With_Valid_Id_Returns_StatusCode_Ok_And_The_Account_Data()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 3;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        var testAccountEntity = generatedEntities[0];

        // Act
        var response = await httpClient.GetAsync(string.Format(AccountEndpoints.AccountWithId, testAccountEntity.Id));
        var responseAccountDto = await response.Content.ReadFromJsonAsync<AccountDTO>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseAccountDto);
        Assert.Multiple(() =>
        {
            Assert.Equal(testAccountEntity.Id.ToString(), responseAccountDto.Id.ToString());
            Assert.Equal(testAccountEntity.Currency.ToString(), responseAccountDto.Currency);
            Assert.Equal(testAccountEntity.AccountType.ToString(), responseAccountDto.AccountType);
            Assert.Equal(testAccountEntity.CreationDate.ToString(DateTimeStringFormat), responseAccountDto.CreationDate);
            Assert.Equal(testAccountEntity.Total, responseAccountDto.Total);
            Assert.Equal(testAccountEntity.Description, responseAccountDto.Description);
        });

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_Single_Account_With_Non_Existing_AccountId_Returns_StatusCode_NotFound()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 3;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        // Act
        var response = await httpClient.GetAsync(string.Format(AccountEndpoints.AccountWithId, Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_Single_Account_With_Transactions_With_Valid_AccountId_Returns_StatusCode_Ok_And_The_Account_Data()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 3;
        int generateTransactionsCount = 5;

        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);
        var testAccountEntity = generatedEntities[0];

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);

        var transactionGenerator = new TransactionDataGenerator(testAccountEntity.Id);

        var generatedTransactions = transactionGenerator.GenerateTransactionEntities(generateTransactionsCount);
        testAccountEntity.Transactions = generatedTransactions;

        await appDbContext.SaveChangesAsync();

        // Act
        var response = await httpClient.GetAsync(string.Format(AccountEndpoints.AccountWithIdAndTransactions, testAccountEntity.Id));
        var responseAccountDtoWithTransactions = await response.Content.ReadFromJsonAsync<AccountWithTransactionsDTO>();

        // Assert

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseAccountDtoWithTransactions);
        Assert.NotNull(responseAccountDtoWithTransactions.Transactions.Items);
        Assert.Equal(generateTransactionsCount, responseAccountDtoWithTransactions.Transactions.Items.Count());

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_All_User_Accounts_Returns_StatusCode_Ok_And_Paginated_Accounts_Data()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 7;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();
        
        // Act
        var response = await httpClient.GetAsync(AccountEndpoints.AccountBase);
        var queryResponse = await response.Content.ReadFromJsonAsync<QueryResponse<AccountDTO>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(queryResponse);
        Assert.Equal(generateAccountsCount, queryResponse?.ItemsCount);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Update_Existing_Account_With_Valid_Data_Returns_StatusCode_Ok_And_Updated_Account_Data()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        var testAccountEntity = generatedEntities[0];

        var updateAccountDto = accountDataGenerator.GenerateUpdateAccountDTO(testAccountEntity.Id.ToString());

        // Act

        var response = await httpClient.PutAsJsonAsync(AccountEndpoints.AccountBase, updateAccountDto);
        var responseAccountDto = await response.Content.ReadFromJsonAsync<AccountDTO>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseAccountDto);
        Assert.Multiple(() =>
        {
            Assert.Equal(updateAccountDto.Id, responseAccountDto.Id.ToString());
            Assert.Equal(updateAccountDto.Name, responseAccountDto.Name);
            Assert.Equal(updateAccountDto.Currency, responseAccountDto.Currency);
            Assert.Equal(updateAccountDto.AccountType, responseAccountDto.AccountType);
            Assert.Equal(updateAccountDto.Description, responseAccountDto.Description);
        });

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Update_Existing_Account_With_Another_Account_Name_Returns_StatusCode_BadRequest_With_Error()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        var testAccountEntity = generatedEntities[0];
        var testAccountEntity2 = generatedEntities[1];

        var updateAccountDto = accountDataGenerator.GenerateUpdateAccountDTO(testAccountEntity.Id.ToString()) with { Name = testAccountEntity2.Name };

        // Act

        var response = await httpClient.PutAsJsonAsync(AccountEndpoints.AccountBase, updateAccountDto);
        var problemResult = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemResult);
        Assert.Single(problemResult.Errors, error =>
        {
            return error.Key == ErrorMessages.DuplicateName.Code &&
                   error.Value.First() == string.Format(ErrorMessages.DuplicateName.Description, updateAccountDto.Name);
        });


        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_With_Existing_AccountId_Returns_StatusCode_NoContent()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        var testAccountEntity = generatedEntities[0];

        // Act

        var response = await httpClient.DeleteAsync(string.Format(AccountEndpoints.AccountWithId, testAccountEntity.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var deletedAccountExists = await appDbContext.Accounts.AnyAsync(a => a.Id == testAccountEntity.Id);
        Assert.False(deletedAccountExists);

        // Cleanup
        appDbContext.Remove(generatedEntities[1]);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_With_NonExisting_AccountId_Returns_StatusCode_NotFound()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);
        await appDbContext.SaveChangesAsync();

        // Act
        var response = await httpClient.DeleteAsync(string.Format(AccountEndpoints.AccountWithId, Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }
}