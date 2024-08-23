namespace PersonalFinancialManager.Application.Queries;

public record QueryModel
(
    string? Search,
    string? Order,
    string? OrderBy,
    int? Page,
    int? ItemsPerPage
);