namespace PersonalFinancialManager.WebApi.Filters;

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class ModelValidationFilter<TModel> : IEndpointFilter where TModel : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ICollection<ValidationResult>? validationResults = [];

        var model = context.Arguments.FirstOrDefault(a => a?.GetType() == typeof(TModel));

        if (!Validator.TryValidateObject(model!, new ValidationContext(model!), validationResults, true))
        {
           var errors = validationResults
                .SelectMany(validationResult => validationResult.MemberNames, (validationResult, memberName) => new { memberName, validationResult.ErrorMessage })
                .GroupBy(x => x.memberName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage!).ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}