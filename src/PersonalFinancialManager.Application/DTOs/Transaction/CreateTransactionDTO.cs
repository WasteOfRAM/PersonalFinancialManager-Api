namespace PersonalFinancialManager.Application.DTOs.Transaction;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;
using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class CreateTransactionDTO
{
    [Required]
    [GuidDataType]
    public required string AccountId { get; set; }

    [Required]
    [EnumDataType(typeof(TransactionType))]
    public required string TransactionType { get; set; }

    [Precision(DecimalPrecisionConstant.Integer, DecimalPrecisionConstant.Fraction)]
    [Range(0.0, double.PositiveInfinity)]
    public decimal Amount { get; set; }

    [StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    public string? Description { get; set; }
}
