namespace PersonalFinancialManager.WebApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.WebApi.Filters;
using System.Security.Claims;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder transactionRoutes = routes.MapGroup("transaction");

        transactionRoutes.MapPost("/", async Task<Results<Ok<TransactionDTO>, ValidationProblem>> (CreateTransactionDTO createTransactionDTO, ITransactionService transactionService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<TransactionDTO> serviceResult = await transactionService.CreateAsync(createTransactionDTO, userId!);

            return serviceResult.Success ? TypedResults.Ok(serviceResult.Data) :
                                           TypedResults.ValidationProblem(serviceResult.Errors!);
        })
            .MapToApiVersion(1)
            .AddEndpointFilter<ModelValidationFilter<CreateTransactionDTO>>();
    }
}
