namespace PersonalFinancialManager.WebApi.Endpoints;

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

        accountRoutes.MapPost("create", async Task<Results<Ok<AccountDTO>, ValidationProblem>> (HttpContext context, CreateAccountDTO createAccountDTO, IAccountService accountService) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<AccountDTO> serviceResult = await accountService.CreateAsync(userId!, createAccountDTO);

            return serviceResult.Success ? TypedResults.Ok(serviceResult.Data) :
                                           TypedResults.ValidationProblem(serviceResult.Errors!);
        })
            .MapToApiVersion(1)
            .AddEndpointFilter<ModelValidationFilter<CreateAccountDTO>>();
    }
}
