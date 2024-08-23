namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Core.Enumerations;

public record TransactionDTO
(
    Guid Id,
    string TransactionType,
    decimal Amount,
    string CreationDate,
    string? Description,
    Guid AccountId
);