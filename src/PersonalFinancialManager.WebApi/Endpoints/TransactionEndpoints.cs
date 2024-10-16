namespace PersonalFinancialManager.WebApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using PersonalFinancialManager.Application.DTOs.Transaction;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.Queries;
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

        transactionRoutes.MapGet("/{id:Guid}", async Task<Results<Ok<TransactionDTO>, NotFound>> (Guid id, ITransactionService transactionService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<TransactionDTO> serviceResult = await transactionService.GetAsync(id, userId!);

            if (!serviceResult.Success)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(serviceResult.Data);
        })
        .MapToApiVersion(1);

        transactionRoutes.MapGet("/", async ([AsParameters] QueryModel query, ITransactionService transactionService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = await transactionService.GetAllAsync(query, userId!);

            return TypedResults.Ok(transactions.Data);
        })
            .MapToApiVersion(1);

        transactionRoutes.MapPut("/", async Task<Results<Ok<TransactionDTO>, NotFound, ValidationProblem>> (UpdateTransactionDTO updateTransactionDTO, ITransactionService transactionService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<TransactionDTO> serviceResult = await transactionService.UpdateAsync(updateTransactionDTO, userId!);

            if (!serviceResult.Success) 
            {
                return serviceResult.Errors == null ? TypedResults.NotFound() : TypedResults.ValidationProblem(serviceResult.Errors);
            }

            return TypedResults.Ok(serviceResult.Data);
        })
            .MapToApiVersion(1)
            .AddEndpointFilter<ModelValidationFilter<UpdateTransactionDTO>>();

        transactionRoutes.MapDelete("/{id:Guid}", async Task<Results<NoContent, NotFound, ValidationProblem>> (Guid id, ITransactionService transactionService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult serviceResult = await transactionService.DeleteAsync(id, userId!);

            if (!serviceResult.Success)
            {
                return serviceResult.Errors == null ? TypedResults.NotFound() : TypedResults.ValidationProblem(serviceResult.Errors);
            }

            return TypedResults.NoContent();
        })
            .MapToApiVersion(1);
    }
}
