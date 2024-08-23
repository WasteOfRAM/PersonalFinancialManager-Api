namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public record UpdateAccountDTO
(
    [property: Required] [property: GuidDataType]
    string Id,

    [property: Required] [property: StringLength(maximumLength: AccountConstants.NameMaxLength)]
    string Name,

    [property: Required] [property: StringLength(maximumLength: AccountConstants.CurrencyMaxLength)]
    string Currency,

    [property: Required] [property: EnumDataType(typeof(AccountType))]
    string AccountType,

    [property: StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    string? Description
);