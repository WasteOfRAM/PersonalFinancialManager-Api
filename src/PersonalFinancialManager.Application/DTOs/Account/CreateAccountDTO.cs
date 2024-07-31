namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

public class CreateAccountDTO
{
    [Required]
    [StringLength(maximumLength: 10)]
    public string Name { get; set; } = null!;

    [Required]
    public string Currency { get; set; } = null!;

    [Required]
    [EnumDataType(typeof(AccountType))]
    public string AccountType { get; set; } = null!;

    public decimal? Total { get; set; }

    public string? Description { get; set; }
}
