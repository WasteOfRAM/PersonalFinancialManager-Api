namespace PersonalFinancialManager.UnitTests.ServiceTests;

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
    }

    [Fact]
    public async Task CreateAsync_With_Duplicate_AccountName_Returns_ResultSuccess_False_With_Error()
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
}
