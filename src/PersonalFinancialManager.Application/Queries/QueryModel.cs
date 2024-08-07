namespace PersonalFinancialManager.Application.Queries;

public class QueryModel
{
    public string? Search { get; set; }

    public string? Order { get; set; }

    public string? OrderBy { get; set; }

    public int? Page { get; set; }

    public int? ItemsPerPage { get; set; }
}