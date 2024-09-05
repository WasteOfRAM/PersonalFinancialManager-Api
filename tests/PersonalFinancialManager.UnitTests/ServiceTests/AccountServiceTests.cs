namespace PersonalFinancialManager.UnitTests.ServiceTests;

using AutoFixture;
using NSubstitute.ReturnsExtensions;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.Services;
using PersonalFinancialManager.Core.Entities;
using System.Linq.Expressions;

public class AccountServiceTests
{
    private readonly IAccountRepository accountRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly AccountService accountService;

    private readonly Fixture fixture;

    public AccountServiceTests()
    {
        accountRepository = Substitute.For<IAccountRepository>();
        transactionRepository = Substitute.For<ITransactionRepository>();

        accountService = new AccountService(accountRepository, transactionRepository);

        fixture = new Fixture();

        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateAsync_With_Duplicate_AccountName_Returns_ResultSuccess_False_With_Error_With_Duplicate_AccountName_Returns_ResultSuccess_False_With_Error()
    {
        // Arrange
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(true);

        var createAccountDTO = fixture.Create<CreateAccountDTO>();
        var userId = Guid.NewGuid();

        // Act
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.NotEmpty(result.Errors);
        });
    }

    [Fact]
    public async Task CreateAsync_With_Unique_Name_And_Null_Total_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Create<CreateAccountDTO>() with { Total = null };

        var userId = Guid.NewGuid();

        // Act
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

        // Assert
        Assert.Multiple(() =>
        {
            accountRepository.ReceivedWithAnyArgs().AddAsync(Arg.Any<Account>());
            accountRepository.Received().SaveAsync();
            transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>());

            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task CreateAsync_With_Unique_Name_And_Zero_Total_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Build<CreateAccountDTO>()
            .With(a => a.Total, 0.0m)
            .Create();

        var userId = Guid.NewGuid();

        // Act
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

        //Assert
        Assert.Multiple(() =>
        {
            accountRepository.Received().AddAsync(Arg.Any<Account>());
            accountRepository.Received().SaveAsync();
            transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>());

            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task CreateAsync_With_Unique_Name_And_GreaterThan_Zero_Total_Returns_ResultSuccess_True_With_Data_And_Calls_TransactionRepository_AddAsync()
    {
        // Arrange
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Build<CreateAccountDTO>()
            .With(a => a.Total, 79797.111m)
            .Create();

        var userId = Guid.NewGuid();

        // Act
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

        // Assert
        Assert.Multiple(() =>
        {
            accountRepository.Received().AddAsync(Arg.Any<Account>());
            accountRepository.Received().SaveAsync();
            transactionRepository.Received().AddAsync(Arg.Any<Transaction>());

            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task DeleteAsync_With_Non_Existing_Id_Or_Not_Owning_User_Returns_ResultSuccess_False()
    {
        // Arrange
        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNull();

        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await accountService.DeleteAsync(id, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);

            accountRepository.DidNotReceive().Delete(Arg.Any<Account>());
            accountRepository.DidNotReceive().SaveAsync();
        });
    }

    [Fact]
    public async Task DeleteAsync_With_Existing_Id_And_Owning_User_Returns_ResultSuccess_True()
    {
        // Arrange
        var accountEntity = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(accountEntity);

        // Act
        var result = await accountService.DeleteAsync(accountEntity.Id, accountEntity.AppUserId.ToString());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);

            accountRepository.Received().Delete(accountEntity);
            accountRepository.Received().SaveAsync();
        });
    }

    [Fact]
    public async Task GetAllAsync_Without_Any_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var queryModel = new QueryModel(null, null, null, null, null);
        var queryResult = fixture.Create<QueryResult<Account>>();

        accountRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await accountService.GetAllAsync(queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Items);
        });
    }

    [Fact]
    public async Task GetAllAsync_With_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var queryModel = new QueryModel("Bank", "DESC", "Name", 2, 5);
        var queryResult = fixture.Create<QueryResult<Account>>();

        accountRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await accountService.GetAllAsync(queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Items?.ToArray());
        });
    }

    [Fact]
    public async Task GetAsync_With_Non_Existing_Id_Or_User_Not_Owner_Returns_ResultSuccess_False_With_Null_Data_And_Errors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        accountRepository.GetAsync(e => e.AppUserId.ToString() == userId && e.Id == id).ReturnsNull();

        // Act
        var result = await accountService.GetAsync(id, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task GetAsync_With_Existing_Id_And_Owning_UserId_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var account = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(account);

        // Act
        var result = await accountService.GetAsync(id, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task GetWithTransactionsAsync_Non_Existing_Id_Or_User_Not_Owner_Returns_ResultSuccess_False_With_Null_Data_And_Errors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var queryModel = fixture.Create<QueryModel>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNullForAnyArgs();

        // Act
        var result = await accountService.GetWithTransactionsAsync(id, queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);
        });
    }

    [Fact]
    public async Task GetWithTransactionsAsync_With_Existing_Id_And_Owning_UserId_Without_Any_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var queryModel = new QueryModel(null, null, null, null, null);
        var account = fixture.Create<Account>();
        var queryResult = fixture.Create<QueryResult<Transaction>>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(account);
        transactionRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await accountService.GetWithTransactionsAsync(id, queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data.Transactions.Items?.ToArray());
        });
    }

    [Fact]
    public async Task GetWithTransactionsAsync_With_Existing_Id_And_Owning_UserId_With_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var queryModel = fixture.Create<QueryModel>();

        var account = fixture.Create<Account>();
        var queryResult = fixture.Create<QueryResult<Transaction>>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(account);
        transactionRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await accountService.GetWithTransactionsAsync(id, queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data.Transactions.Items?.ToArray());
        });
    }

    [Fact]
    public async Task UpdateAsync_Non_Existing_Id_Or_User_Not_Owner_Returns_ResultSuccess_False_With_Null_Data_And_Errors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var updateAccountDTO = fixture.Create<UpdateAccountDTO>() with { Id = id.ToString() };

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNullForAnyArgs();

        // Act
        var result = await accountService.UpdateAsync(updateAccountDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Null(result.Errors);

            accountRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<Account>());
            accountRepository.DidNotReceive().SaveAsync();
        });
    }

    [Fact]
    public async Task UpdateAsync_With_Existing_Id_And_Owning_UserId_With_DuplicateName_ResultSuccess_False_With_Errors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var updateAccountDTO = fixture.Create<UpdateAccountDTO>() with { Id = id.ToString() };

        var account = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(account);
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(true);

        // Act
        var result = await accountService.UpdateAsync(updateAccountDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);

            accountRepository.DidNotReceiveWithAnyArgs().Update(Arg.Any<Account>());
            accountRepository.DidNotReceive().SaveAsync();
        });
    }

    [Fact]
    public async Task UpdateAsync_With_Existing_Id_And_Owning_UserId_With_UniqueName_ResultSuccess_True_With_Data()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var updateAccountDTO = fixture.Create<UpdateAccountDTO>() with { Id = id.ToString() };

        var account = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(account);
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(false);

        // Act
        var result = await accountService.UpdateAsync(updateAccountDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);

            accountRepository.Received().Update(Arg.Any<Account>());
            accountRepository.Received().SaveAsync();
        });
    }
}