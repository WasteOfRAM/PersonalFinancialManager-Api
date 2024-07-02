namespace PersonalFinancialManager.Application.ServiceModels;

public class ServiceResult
{
    public bool Success { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }
}

public class ServiceResult<T> : ServiceResult where T : class
{
    public T? Data { get; set; }
}
