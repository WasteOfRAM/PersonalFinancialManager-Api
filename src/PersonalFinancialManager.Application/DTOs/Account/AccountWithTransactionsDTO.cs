namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Queries;

public record AccountWithTransactionsDTO
(
    Guid Id,
    string Name,
    string Currency,
    string AccountType,
    string CreationDate,
    decimal Total,
    string? Description,
    QueryResponse<TransactionDTO> Transactions
);