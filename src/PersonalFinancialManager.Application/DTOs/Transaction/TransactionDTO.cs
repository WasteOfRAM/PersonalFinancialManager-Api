namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Core.Enumerations;

public class TransactionDTO
{
    public Guid Id { get; set; }

    public required string TransactionType { get; set; }

    public decimal Amount { get; set; }

    public required string CreationDate { get; set; }

    public string? Description { get; set; }

    public Guid AccountId { get; set; }
}
