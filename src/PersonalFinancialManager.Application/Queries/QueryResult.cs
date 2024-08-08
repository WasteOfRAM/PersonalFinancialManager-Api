namespace PersonalFinancialManager.Application.Queries;

public class QueryResult<T>
{
    public int ItemsCount { get; set; }

    public IEnumerable<T> Items { get; set; } = [];
}
