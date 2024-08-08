namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

public class CreateAccountDTO
{
    [Required]
    [StringLength(maximumLength: 10)]
    public required string Name { get; set; }

    [Required]
    public required string Currency { get; set; }

    [Required]
    [EnumDataType(typeof(AccountType))]
    public required string AccountType { get; set; }

    [Range(0.0, double.PositiveInfinity)]
    public decimal? Total { get; set; }

    [StringLength(maximumLength: 100, MinimumLength = 2)]
    public string? Description { get; set; }
}
