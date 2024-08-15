namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

public class CreateTransactionDTO
{
    [Required]
    [GuidDataType]
    public required string AccountId { get; set; }

    [Required]
    [EnumDataType(typeof(TransactionType))]
    public required string TransactionType { get; set; }

    [Range(0.0, double.PositiveInfinity)]
    public decimal Amount { get; set; }

    [StringLength(maximumLength: 100, MinimumLength = 2)]
    public string? Description { get; set; }
}
