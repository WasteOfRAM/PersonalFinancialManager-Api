﻿namespace PersonalFinancialManager.Application.Queries;

public class QueryResponse<T>
{
    public string? Search { get; set; }

    public int CurrentPage { get; set; }

    public int? ItemsCount { get; set; }

    public int? ItemsPerPage { get; set; }

    public string? Order { get; set; }

    public string? OrderBy { get; set; }

    public IEnumerable<T>? Items { get; set; }
}
