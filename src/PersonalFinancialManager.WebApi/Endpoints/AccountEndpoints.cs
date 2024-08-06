namespace PersonalFinancialManager.WebApi.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using PersonalFinancialManager.Application.DTOs.Account;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Application.ServiceModels;
using PersonalFinancialManager.WebApi.Filters;
using System.Security.Claims;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder accountRoutes = routes.MapGroup("account");

        accountRoutes.MapPost("/", async Task<Results<Ok<AccountDTO>, ValidationProblem>> (CreateAccountDTO createAccountDTO, IAccountService accountService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<AccountDTO> serviceResult = await accountService.CreateAsync(userId!, createAccountDTO);

            return serviceResult.Success ? TypedResults.Ok(serviceResult.Data) :
                                           TypedResults.ValidationProblem(serviceResult.Errors!);
        })
            .MapToApiVersion(1)
            .AddEndpointFilter<ModelValidationFilter<CreateAccountDTO>>();


        accountRoutes.MapGet("/{id:Guid}", async Task<Results<Ok<AccountDTO>, NotFound>> (Guid id, IAccountService accountService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<AccountDTO> serviceResult = await accountService.GetAsync(id, userId!);

            if (!serviceResult.Success)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(serviceResult.Data);
        })
            .MapToApiVersion(1);

        accountRoutes.MapPut("/", async Task<Results<Ok<AccountDTO>, NotFound, ValidationProblem>> (UpdateAccountDTO updateAccountDTO, IAccountService accountService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<AccountDTO> serviceResult = await accountService.UpdateAsync(updateAccountDTO, userId!);

            if (!serviceResult.Success)
            {
                if (serviceResult.Errors == null)
                {
                    return TypedResults.NotFound();
                }
                else
                {
                    return TypedResults.ValidationProblem(serviceResult.Errors);
                }
            }

            return TypedResults.Ok(serviceResult.Data);
        })
            .MapToApiVersion(1)
            .AddEndpointFilter<ModelValidationFilter<UpdateAccountDTO>>();

        accountRoutes.MapDelete("/{id:Guid}", async Task<Results<NoContent, NotFound>> (Guid id, IAccountService accountService, HttpContext context) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult result = await accountService.DeleteAsync(id, userId!);

            if (!result.Success)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.NoContent();
        })
            .MapToApiVersion(1);
    }
}
