namespace PersonalFinancialManager.Application.DTOs.Account;

public class AccountDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public string CreationDate { get; set; } = null!;

    public decimal Total { get; set; }

    public string? Description { get; set; }
}
