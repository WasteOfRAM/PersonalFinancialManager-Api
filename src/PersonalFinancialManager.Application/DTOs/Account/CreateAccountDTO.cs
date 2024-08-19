namespace PersonalFinancialManager.Application.DTOs.Account;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class CreateAccountDTO
{
    [Required]
    [StringLength(maximumLength: AccountConstants.NameMaxLength)]
    public required string Name { get; set; }

    [Required]
    [StringLength(maximumLength: AccountConstants.CurrencyMaxLength)]
    public required string Currency { get; set; }

    [Required]
    [EnumDataType(typeof(AccountType))]
    public required string AccountType { get; set; }

    [Precision(DecimalPrecisionConstant.Integer, DecimalPrecisionConstant.Fraction)]
    [Range(0.0, double.PositiveInfinity)]
    public decimal? Total { get; set; }

    [StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    public string? Description { get; set; }
}