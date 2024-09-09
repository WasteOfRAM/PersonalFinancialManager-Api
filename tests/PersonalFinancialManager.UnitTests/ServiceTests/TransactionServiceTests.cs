namespace PersonalFinancialManager.UnitTests.ServiceTests;

using PersonalFinancialManager.Application.Interfaces.Repositories;
using PersonalFinancialManager.Application.Services;

public class TransactionServiceTests
{
    private readonly IAccountRepository accountRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly TransactionService transactionService;

    private readonly Fixture fixture;

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
}
