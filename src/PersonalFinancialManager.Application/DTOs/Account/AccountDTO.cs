namespace PersonalFinancialManager.Application.DTOs.Account;

public class AccountDTO
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Currency { get; set; }

    public required string AccountType { get; set; }

    public required string CreationDate { get; set; }

    public decimal Total { get; set; }

    public string? Description { get; set; }
}
