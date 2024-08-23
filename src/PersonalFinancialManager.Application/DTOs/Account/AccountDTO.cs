namespace PersonalFinancialManager.Application.DTOs.Account;

public record AccountDTO(

    Guid Id,
    string Name,
    string Currency,
    string AccountType,
    string CreationDate,
    decimal Total,
    string? Description
);
