namespace PersonalFinancialManager.IntegrationTests.DataGenerators;

using Bogus;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Core.Entities;
using PersonalFinancialManager.Core.Enumerations;
using System.Globalization;

using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;
using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class AccountDataGenerator(Guid userId)
{
    private static readonly decimal minValue = decimal.Parse(DecimalRangeMinimumValue, CultureInfo.InvariantCulture);
    private static readonly decimal maxValue = decimal.Parse(DecimalRangeMaximumValue, CultureInfo.InvariantCulture);

    // Id's are not generated (bogus adds the default Guid all 0's) so that the database can generate them and manage the order.
    private readonly Faker<Account> accountEntities = new Faker<Account>()
        .RuleFor(p => p.Name, f => f.Random.String2(AccountConstants.NameMaxLength))
        .RuleFor(p => p.Currency, f => f.Random.Enum<Currency>())
        .RuleFor(p => p.AccountType, f => f.Random.Enum<AccountType>())
        .RuleFor(p => p.CreationDate, f => f.Date.Past(1, DateTime.UtcNow))
        .RuleFor(p => p.Total, f => f.Finance.Amount(minValue, maxValue, DecimalPrecisionConstant.Scale))
        .RuleFor(p => p.Description, f => f.Random.Words().OrNull(f, 0.3f))
        .RuleFor(p => p.AppUserId, userId);

    private readonly Faker<CreateAccountDTO> createAccountDTOs = new Faker<CreateAccountDTO>()
        .CustomInstantiator(f => new CreateAccountDTO(
            f.Random.String2(AccountConstants.NameMaxLength),
            f.Random.Enum<Currency>().ToString(),
            f.Random.Enum<AccountType>().ToString(),
            f.Finance.Amount(minValue, maxValue, DecimalPrecisionConstant.Scale).OrNull(f, 0.3f),
            f.Random.Words().OrNull(f)));

    public List<Account> GenerateAccountEntities(int itemsCount = 1)
    {
        return accountEntities.Generate(itemsCount);
    }

    public List<CreateAccountDTO> GenerateCreateAccountDTO(int itemsCount = 1)
    {
        return createAccountDTOs.Generate(itemsCount);
    }
}