namespace PersonalFinancialManager.IntegrationTests;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Infrastructure.Data;
using PersonalFinancialManager.IntegrationTests.DataGenerators;
using PersonalFinancialManager.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using static PersonalFinancialManager.IntegrationTests.Constants.EndpointsV1;

[Collection("Tests collection")]
public class TransactionEndpointsV1Tests(TestsFixture testsFixture)
{
    private readonly HttpClient httpClient = testsFixture.AppFactory.CreateClient();
    private readonly string testUserAccessToken = testsFixture.SeededUserAccessToken;

    private readonly AccountDataGenerator accountDataGenerator = new(Guid.Parse(testsFixture.SeededUserId));

    [Fact]
    public async Task Create_Transaction_With_Valid_Data_Returns_StatusCode_Ok_With_The_Created_Transaction()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        var generatedTestAccount = accountDataGenerator.GenerateAccountEntities()[0];

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Accounts.AddAsync(generatedTestAccount);
        await appDbContext.SaveChangesAsync();

        var transactionGenerator = new TransactionDataGenerator(generatedTestAccount.Id);

        var createTransactionDTO = transactionGenerator.GenerateCreateTransactionDTO();

        // Act
        var response = await httpClient.PostAsJsonAsync(TransactionEndpoints.TransactionBase, createTransactionDTO);
        var responseTransactionDTO = await response.Content.ReadFromJsonAsync<TransactionDTO>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseTransactionDTO);

        var isTransactionCreated = await appDbContext.Transactions.AnyAsync(t => t.Id == responseTransactionDTO.Id);
        Assert.True(isTransactionCreated);

        Assert.Multiple(() =>
        {
            Assert.Equal(createTransactionDTO.AccountId, responseTransactionDTO.AccountId.ToString());
            Assert.Equal(createTransactionDTO.TransactionType, responseTransactionDTO.TransactionType);
            Assert.Equal(createTransactionDTO.Description, responseTransactionDTO.Description);
            Assert.Equal(createTransactionDTO.Amount, responseTransactionDTO.Amount);
        });

        // Cleanup
        appDbContext.Remove(generatedTestAccount);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_Transaction_With_Invalid_Data_Returns_StatusCode_BadRequest_With_Errors()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        var generatedTestAccount = accountDataGenerator.GenerateAccountEntities()[0];

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDbContext.Accounts.AddAsync(generatedTestAccount);
        await appDbContext.SaveChangesAsync();

        var transactionGenerator = new TransactionDataGenerator(generatedTestAccount.Id);

        var createTransactionDTO = transactionGenerator.GenerateCreateTransactionDTO() with { TransactionType = "NotExistingType"};

        // Act
        var response = await httpClient.PostAsJsonAsync(TransactionEndpoints.TransactionBase, createTransactionDTO);
        var problemResult = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemResult?.Errors);

        // Cleanup
        appDbContext.Remove(generatedTestAccount);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_Single_Transaction_With_Valid_Id_Returns_StatusCode_Ok_And_The_Transaction_Data()
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

        var testTransactionEntity = generatedTransactions[0];

        // Act
        var response = await httpClient.GetAsync(string.Format(TransactionEndpoints.TransactionWithId, testTransactionEntity.Id));
        var responseTransactionDTO = await response.Content.ReadFromJsonAsync<TransactionDTO>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseTransactionDTO);
        Assert.Multiple(() =>
        {
            Assert.Equal(testTransactionEntity.AccountId, responseTransactionDTO.AccountId);
            Assert.Equal(testTransactionEntity.TransactionType.ToString(), responseTransactionDTO.TransactionType);
            Assert.Equal(testTransactionEntity.Description, responseTransactionDTO.Description);
            Assert.Equal(testTransactionEntity.Amount, responseTransactionDTO.Amount);
        });

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_Single_Transaction_With_Invalid_Id_Returns_StatusCode_NotFound()
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

        var testTransactionEntity = generatedTransactions[0];

        // Act
        var response = await httpClient.GetAsync(string.Format(TransactionEndpoints.TransactionWithId, Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_All_User_Transactions_Returns_StatusCode_Ok_And_Paginated_Transactions_Data()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 3;
        int generateTransactionsCount = 5;

        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);

        foreach (var account in generatedEntities)
        {
            var transactionGenerator = new TransactionDataGenerator(account.Id);

            account.Transactions = transactionGenerator.GenerateTransactionEntities(generateTransactionsCount);
        }

        await appDbContext.SaveChangesAsync();

        // Act
        var response = await httpClient.GetAsync(string.Format(TransactionEndpoints.TransactionBase));
        var queryResponse = await response.Content.ReadFromJsonAsync<QueryResponse<TransactionDTO>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(queryResponse);
        Assert.Equal(generateTransactionsCount * generateAccountsCount, queryResponse.ItemsCount);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Update_Existing_Transaction_With_Valid_Data_Returns_StatusCode_Ok_And_Updated_Transaction_Data()
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

        var testTransactionEntity = generatedTransactions[0];

        var updateTransactionDTO = transactionGenerator.GenerateUpdateTransactionDTO(testTransactionEntity.Id.ToString());

        // Act
        var response = await httpClient.PutAsJsonAsync(TransactionEndpoints.TransactionBase, updateTransactionDTO);
        var responseTransactionDTO = await response.Content.ReadFromJsonAsync<TransactionDTO>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseTransactionDTO);

        Assert.Multiple(() =>
        {
            Assert.Equal(updateTransactionDTO.Id, responseTransactionDTO.Id.ToString());
            Assert.Equal(updateTransactionDTO.AccountId, responseTransactionDTO.AccountId.ToString());
            Assert.Equal(updateTransactionDTO.Amount, responseTransactionDTO.Amount);
            Assert.Equal(updateTransactionDTO.TransactionType, responseTransactionDTO.TransactionType);
            Assert.Equal(updateTransactionDTO.Description, responseTransactionDTO.Description);
        });

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Update_With_NonExisting_TransactionId_Returns_StatusCode_NotFound()
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

        var updateTransactionDTO = transactionGenerator.GenerateUpdateTransactionDTO(Guid.NewGuid().ToString());

        // Act
        var response = await httpClient.PutAsJsonAsync(TransactionEndpoints.TransactionBase, updateTransactionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_With_Existing_TransactionId_Returns_StatusCode_NoContent()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        int generateTransactionsCount = 3;

        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);
        var testAccountEntity = generatedEntities[0];

        testAccountEntity.Total = 0.0m;

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);

        var transactionGenerator = new TransactionDataGenerator(testAccountEntity.Id);

        var generatedTransactions = transactionGenerator.GenerateTransactionEntities(generateTransactionsCount);
        testAccountEntity.Transactions = generatedTransactions;

        await appDbContext.SaveChangesAsync();

        var testTransactionEntity = generatedTransactions[0];

        // Act
        var response = await httpClient.DeleteAsync(string.Format(TransactionEndpoints.TransactionWithId, testTransactionEntity.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var deletedTransactionExists = await appDbContext.Transactions.AnyAsync(t => t.Id == testTransactionEntity.Id);
        Assert.False(deletedTransactionExists);

        // Cleanup
        appDbContext.Entry(testAccountEntity).State = EntityState.Detached;
        generatedEntities = await appDbContext.Accounts.ToListAsync();
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_With_NonExisting_TransactionId_Returns_StatusCode_NotFound()
    {
        // Arrange
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserAccessToken);

        int generateAccountsCount = 2;
        int generateTransactionsCount = 3;

        var generatedEntities = accountDataGenerator.GenerateAccountEntities(generateAccountsCount);
        var testAccountEntity = generatedEntities[0];

        testAccountEntity.Total = 0.0m;

        using var scope = testsFixture.AppFactory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.AddRangeAsync(generatedEntities);

        var transactionGenerator = new TransactionDataGenerator(testAccountEntity.Id);

        var generatedTransactions = transactionGenerator.GenerateTransactionEntities(generateTransactionsCount);
        testAccountEntity.Transactions = generatedTransactions;

        await appDbContext.SaveChangesAsync();

        var testTransactionEntity = generatedTransactions[0];

        // Act
        var response = await httpClient.DeleteAsync(string.Format(TransactionEndpoints.TransactionWithId, Guid.NewGuid().ToString()));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);


        // Cleanup
        appDbContext.RemoveRange(generatedEntities);
        await appDbContext.SaveChangesAsync();
    }
}
