namespace PersonalFinancialManager.Application.Queries;

public record QueryResponse<T>
(
    string? Search,
    int? ItemsCount,
    int CurrentPage,
    int? ItemsPerPage,
    string? OrderBy,
    string? Order,
    IEnumerable<T>? Items
);