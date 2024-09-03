namespace PersonalFinancialManager.UnitTests.ServiceTests;

using NSubstitute.ReturnsExtensions;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces.Repositories;
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
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(true);

        var createAccountDTO = fixture.Create<CreateAccountDTO>();
        var userId = Guid.NewGuid();

        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

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
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Create<CreateAccountDTO>() with { Total = null };

        var userId = Guid.NewGuid();
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

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
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Build<CreateAccountDTO>()
            .With(a => a.Total, 0.0m)
            .Create();

        var userId = Guid.NewGuid();
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

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
        accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(false);
        var createAccountDTO = fixture.Build<CreateAccountDTO>()
            .With(a => a.Total, 79797.111m)
            .Create();

        var userId = Guid.NewGuid();
        var result = await accountService.CreateAsync(userId.ToString(), createAccountDTO);

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
        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNull();

        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var result = await accountService.DeleteAsync(id, userId);

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
        var accountEntity = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(accountEntity);

        var result = await accountService.DeleteAsync(accountEntity.Id, accountEntity.AppUserId.ToString());

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);

            accountRepository.Received().Delete(accountEntity);
            accountRepository.Received().SaveAsync();
        });
    }
}
