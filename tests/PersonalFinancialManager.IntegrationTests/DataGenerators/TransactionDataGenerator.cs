namespace PersonalFinancialManager.IntegrationTests.DataGenerators;

using Bogus;
using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System.Globalization;

using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;
using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class TransactionDataGenerator(Guid accountId)
{
    private static readonly decimal minValue = decimal.Parse(DecimalRangeMinimumValue, CultureInfo.InvariantCulture);
    private static readonly decimal maxValue = decimal.Parse(DecimalRangeMaximumValue, CultureInfo.InvariantCulture);

    private readonly Faker<Transaction> transactionEntities = new Faker<Transaction>()
        .RuleFor(p => p.TransactionType, f => f.Random.Enum<TransactionType>())
        .RuleFor(p => p.Amount, f => f.Finance.Amount(minValue, maxValue, DecimalPrecisionConstant.Scale))
        .RuleFor(p => p.CreationDate, f => f.Date.Past(1, DateTime.UtcNow))
        .RuleFor(p => p.Description, f => f.Random.Words().OrNull(f, 0.3f))
        .RuleFor(p => p.AccountId, accountId);

    private readonly Faker<CreateTransactionDTO> createTransactionDTO = new Faker<CreateTransactionDTO>()
        .CustomInstantiator(f => new CreateTransactionDTO(
            accountId.ToString(),
            f.Random.Enum<TransactionType>().ToString(),
            f.Finance.Amount(minValue, maxValue, DecimalPrecisionConstant.Scale),
            f.Random.Words().OrNull(f, 0.3f)));

    private readonly Faker<UpdateTransactionDTO> updateTransactionDTO = new Faker<UpdateTransactionDTO>()
        .CustomInstantiator(f => new UpdateTransactionDTO(
            Guid.Empty.ToString(),
            accountId.ToString(),
            f.Random.Enum<TransactionType>().ToString(),
            f.Finance.Amount(minValue, maxValue, DecimalPrecisionConstant.Scale),
            f.Random.Words().OrNull(f, 0.3f)));

    public List<Transaction> GenerateTransactionEntities(int itemCount = 1)
    {
        return transactionEntities.Generate(itemCount);
    }

    public CreateTransactionDTO GenerateCreateTransactionDTO()
    {
        return createTransactionDTO.Generate();
    }

    public UpdateTransactionDTO GenerateUpdateTransactionDTO(string transactionId) 
    {
        return updateTransactionDTO.Generate() with { Id = transactionId};
    }
}
