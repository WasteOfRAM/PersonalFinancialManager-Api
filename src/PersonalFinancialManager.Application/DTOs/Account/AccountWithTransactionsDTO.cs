namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Queries;

public class AccountWithTransactionsDTO
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Currency { get; set; }

    public required string AccountType { get; set; }

    public required string CreationDate { get; set; }

    public decimal Total { get; set; }

    public string? Description { get; set; }

    public required QueryResponse<TransactionDTO> Transactions { get; set; }
}