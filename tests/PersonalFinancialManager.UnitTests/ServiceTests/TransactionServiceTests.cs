namespace PersonalFinancialManager.UnitTests.ServiceTests;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Queries;
using PersonalFinancialManager.Application.Services;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System.Linq.Expressions;

using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;
using static PersonalFinancialManager.Application.Constants.ApplicationValidationMessages;

public class TransactionServiceTests
{
    private readonly IAccountRepository accountRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly TransactionService transactionService;

    private readonly Fixture fixture;

    public static TheoryData<decimal, decimal, TransactionType> UpdateTransactionData => new()
    {
        { 0.0m, 0.01m, TransactionType.Withdraw },
        { 0.0m, 100.00m, TransactionType.Withdraw },
        { 0.0001m, 999999999999999.9999m, TransactionType.Deposit },
        { 999999999999999.9999m, 0.0001m, TransactionType.Deposit }
    };

    public TransactionServiceTests()
    {
        transactionRepository = Substitute.For<ITransactionRepository>();
        accountRepository = Substitute.For<IAccountRepository>();

        transactionService = new TransactionService(transactionRepository, accountRepository);

        fixture = new Fixture();

        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateAsync_With_Non_Existing_Account_Or_User_Not_Owner_Returns_ResultSuccess_False_With_Error()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createTransactionDTO = fixture.Create<CreateTransactionDTO>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNull();

        // Act

        var result = await transactionService.CreateAsync(createTransactionDTO, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);

            transactionRepository.DidNotReceiveWithAnyArgs().AddAsync(Arg.Any<Transaction>());
            transactionRepository.DidNotReceiveWithAnyArgs().SaveAsync();
            accountRepository.DidNotReceiveWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
        });
    }

    [Fact]
    public async Task CreateAsync_With_Existing_Account_And_User_Owner_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createTransactionDTO = fixture.Create<CreateTransactionDTO>();
        var account = fixture.Create<Account>();

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(account);

        // Act

        var result = await transactionService.CreateAsync(createTransactionDTO, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Errors);

            transactionRepository.ReceivedWithAnyArgs().AddAsync(Arg.Any<Transaction>());
            transactionRepository.ReceivedWithAnyArgs().SaveAsync();
            accountRepository.ReceivedWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
        });
    }

    [Theory]
    [InlineData(0.0001, 999999999999999.9999)]
    [InlineData(999999999999999.9999, 0.0001)]
    public async Task CreateAsync_With_TransactionAmount_That_Will_Exceed_Account_Total_Returns_ResultSuccess_False_With_Error(decimal accountTotal, decimal transactionAmount)
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createTransactionDTO = fixture.Create<CreateTransactionDTO>();
        var account = fixture.Create<Account>();
        account.Total = accountTotal;
        createTransactionDTO = createTransactionDTO with { Amount = transactionAmount, TransactionType = "Deposit" };

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(account);

        // Act

        var result = await transactionService.CreateAsync(createTransactionDTO, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Null(result.Data);
            Assert.Equal(ErrorMessages.AccountTotalMaxValue.Code, result.Errors.First().Key);
            Assert.Equal(string.Format(ErrorMessages.AccountTotalMaxValue.Description, DecimalRangeMaximumValue), result.Errors.First().Value[0]);
        });
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100.00)]
    public async Task CreateAsync_With_TransactionAmount_That_Will_Make_Account_Total_NegativeNumber_Returns_ResultSuccess_False_With_Error(decimal transactionAmount)
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var createTransactionDTO = fixture.Create<CreateTransactionDTO>();
        var account = fixture.Create<Account>();
        account.Total = 0;
        createTransactionDTO = createTransactionDTO with { Amount = transactionAmount, TransactionType = "Withdraw" };

        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).Returns(account);

        // Act

        var result = await transactionService.CreateAsync(createTransactionDTO, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Null(result.Data);
            Assert.Equal(ErrorMessages.AccountTotalMinValue.Code, result.Errors.First().Key);
            Assert.Equal(ErrorMessages.AccountTotalMinValue.Description, result.Errors.First().Value[0]);
        });
    }

    [Fact]
    public async Task DeleteAsync_With_Non_Existing_Transaction_Or_User_Not_Owner_Returns_ResultSuccess_False_Without_Errors()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();

        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsNull();

        // Act

        var result = await transactionService.DeleteAsync(transactionId, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Errors);

            accountRepository.DidNotReceiveWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
            transactionRepository.DidNotReceiveWithAnyArgs().Delete(Arg.Any<Transaction>());
            transactionRepository.DidNotReceive().SaveAsync();
        });
    }

    [Fact]
    public async Task DeleteAsync_With_Existing_Transaction_And_Owning_User_Returns_ResultSuccess_True()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();

        var transaction = fixture.Create<Transaction>();

        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(transaction);

        // Act

        var result = await transactionService.DeleteAsync(transactionId, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);

            accountRepository.ReceivedWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
            transactionRepository.ReceivedWithAnyArgs().Delete(Arg.Any<Transaction>());
            transactionRepository.Received().SaveAsync();
        });
    }

    [Fact]
    public async Task DeleteAsync_Deleting_Transaction_That_Was_NOT_Latest_Returns_ResultSuccess_False_With_Error()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();

        var transaction = fixture.Create<Transaction>();

        transactionRepository.AnyAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(true);
        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(transaction);

        // Act

        var result = await transactionService.DeleteAsync(transactionId, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(ErrorMessages.ForbiddenTransactionDeletion.Code, result.Errors.First().Key);
            Assert.Equal(ErrorMessages.ForbiddenTransactionDeletion.Description, result.Errors.First().Value[0]);

            accountRepository.DidNotReceiveWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
            transactionRepository.DidNotReceiveWithAnyArgs().Delete(Arg.Any<Transaction>());
            transactionRepository.DidNotReceive().SaveAsync();
        });
    }

    [Fact]
    public async Task GetAllAsync_Without_Any_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var queryModel = new QueryModel(null, null, null, null, null);
        var queryResult = fixture.Create<QueryResult<Transaction>>();

        transactionRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await transactionService.GetAllAsync(queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Items?.ToArray());
        });
    }

    [Fact]
    public async Task GetAllAsync_With_Queries_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var queryModel = new QueryModel("Coffee", "DESC", "Name", 2, 5);
        var queryResult = fixture.Create<QueryResult<Transaction>>();

        transactionRepository.GetAllAsync().ReturnsForAnyArgs(queryResult);

        // Act
        var result = await transactionService.GetAllAsync(queryModel, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Items?.ToArray());
        });
    }

    [Fact]
    public async Task GetAsync_With_Non_Existing_Transaction_Id_Returns_ResultSuccess_False_Without_Errors_And_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();

        transactionRepository.GetByIdAsync(transactionId).ReturnsNull();

        // Act
        var result = await transactionService.GetAsync(transactionId, userId);

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Errors);
            Assert.Null(result.Data);
        });
    }

    [Fact]
    public async Task GetAsync_With_Existing_Transaction_Id_With_Not_Owning_User_Returns_ResultSuccess_False_Without_Errors_And_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();
        var transaction = fixture.Create<Transaction>();

        transactionRepository.GetByIdAsync(transactionId).Returns(transaction);
        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsNull();

        // Act
        var result = await transactionService.GetAsync(transactionId, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Errors);
            Assert.Null(result.Data);
        });
    }

    [Fact]
    public async Task GetAsync_With_Existing_Transaction_Id_With_Owning_User_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transactionId = Guid.NewGuid();
        var transaction = fixture.Create<Transaction>();
        var account = fixture.Create<Account>();

        transactionRepository.GetByIdAsync(transactionId).Returns(transaction);
        accountRepository.GetAsync(Arg.Any<Expression<Func<Account, bool>>>()).ReturnsForAnyArgs(account);

        // Act
        var result = await transactionService.GetAsync(transactionId, userId);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task UpdateAsync_With_Non_Existing_Transaction_Id_And_User_Not_Owner_Returns_ResultSuccess_False_Without_Errors_And_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var updateTransactionDTO = fixture.Create<UpdateTransactionDTO>();

        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsNullForAnyArgs();

        // Act
        var result = await transactionService.UpdateAsync(updateTransactionDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.Null(result.Errors);
            Assert.Null(result.Data);
        });
    }

    [Fact]
    public async Task UpdateAsync_With_Existing_Transaction_Id_And_User_Owner_Returns_ResultSuccess_True_With_Data()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transaction = fixture.Create<Transaction>();
        var updateTransactionDTO = fixture.Create<UpdateTransactionDTO>();

        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(transaction);

        // Act
        var result = await transactionService.UpdateAsync(updateTransactionDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.Success);
            Assert.Null(result.Errors);
            Assert.NotNull(result.Data);

            accountRepository.Received().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
            transactionRepository.ReceivedWithAnyArgs().Update(transaction);
            transactionRepository.Received().SaveAsync();
        });
    }

    [Fact]
    public async Task UpdateAsync_Updating_Transaction_That_Is_NOT_The_Latest_Returns_ResultSuccess_False_With_Error()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var transaction = fixture.Create<Transaction>();
        var updateTransactionDTO = fixture.Create<UpdateTransactionDTO>();

        transactionRepository.AnyAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(true);
        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(transaction);

        // Act
        var result = await transactionService.UpdateAsync(updateTransactionDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Null(result.Data);
            Assert.Equal(ErrorMessages.ForbiddenTransactionEdit.Code, result.Errors.First().Key);
            Assert.Equal(ErrorMessages.ForbiddenTransactionEdit.Description, result.Errors.First().Value[0]);

            accountRepository.DidNotReceiveWithAnyArgs().UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>());
            transactionRepository.DidNotReceiveWithAnyArgs().Update(transaction);
            transactionRepository.DidNotReceive().SaveAsync();
        });
    }

    [Theory]
    [MemberData(nameof(UpdateTransactionData))]
    public async Task UpdateAsync_With_Existing_Transaction_Id_And_User_Owner_That_Will_Make_Account_Total_RangeInvalid_Returns_ResultSuccess_False_With_Error(decimal accountTotal, decimal updateTransactionAmount, TransactionType transactionType)
    {
        // Arrange
        decimal transactionEntityAmount = 15.00m;

        var userId = Guid.NewGuid().ToString();
        var updateTransactionDTO = fixture.Create<UpdateTransactionDTO>() with { Amount = updateTransactionAmount, TransactionType = transactionType.ToString() };

        var transaction = fixture.Create<Transaction>();
        transaction.TransactionType = TransactionType.Deposit;
        transaction.Amount = transactionEntityAmount;
        transaction.Account!.Total = accountTotal + transactionEntityAmount;

        transactionRepository.GetAsync(Arg.Any<Expression<Func<Transaction, bool>>>()).ReturnsForAnyArgs(transaction);

        accountRepository.When(r => r.UpdateAccountTotal(Arg.Any<Account>(), Arg.Any<decimal>(), Arg.Any<TransactionType>()))
                         .Do(r => transaction.Account!.Total -= transaction.Amount);

        // Act
        var result = await transactionService.UpdateAsync(updateTransactionDTO, userId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Null(result.Data);

            transactionRepository.DidNotReceive().SaveAsync();
        });
    }
}
