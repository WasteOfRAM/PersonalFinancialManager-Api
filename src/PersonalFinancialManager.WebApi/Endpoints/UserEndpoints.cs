namespace PersonalFinancialManager.WebApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces;
using PersonalFinancialManager.Application.ServiceModels;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder userGroup = routes.MapGroup("user");

        userGroup.MapPost("register", async Task<Results<Ok, ValidationProblem>> (CreateUserDTO userDTO, IUserService userService) =>
        {
            ServiceResult result = await userService.CreateAsync(userDTO);

            if (!result.Success)
                return TypedResults.ValidationProblem(result.Errors!);

            return TypedResults.Ok();
        })
            .MapToApiVersion(1)
            .AllowAnonymous();
    }
}
