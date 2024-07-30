namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

public class CreateAccountDTO
{
    public string Name { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public decimal Total { get; set; }

    public string? Description { get; set; }
}
