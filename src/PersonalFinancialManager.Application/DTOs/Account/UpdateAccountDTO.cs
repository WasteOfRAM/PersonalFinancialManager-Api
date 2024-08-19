namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class UpdateAccountDTO
{
    [Required]
    [GuidDataType]
    public required string Id { get; set; }

    [Required]
    [StringLength(maximumLength: AccountConstants.NameMaxLength)]
    public required string Name { get; set; }

    [Required]
    [StringLength(maximumLength: AccountConstants.CurrencyMaxLength)]
    public required string Currency { get; set; }

    [Required]
    [EnumDataType(typeof(AccountType))]
    public required string AccountType { get; set; }

    [StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    public string? Description { get; set; }
}
