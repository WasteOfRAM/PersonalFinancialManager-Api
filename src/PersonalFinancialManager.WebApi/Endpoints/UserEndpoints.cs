namespace PersonalFinancialManager.WebApi.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;
using PersonalFinancialManager.Application.DTOs.Authentication;
using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces.Services;
using PersonalFinancialManager.Application.ServiceModels;
using System.Security.Claims;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder userRoutes = routes.MapGroup("user");

        userRoutes.MapPost("register", async Task<Results<Ok, ValidationProblem>> (CreateUserDTO userDTO, IUserService userService) =>
        {
            ServiceResult serviceResult = await userService.CreateAsync(userDTO);

            return serviceResult.Success ? TypedResults.Ok() :
                                           TypedResults.ValidationProblem(serviceResult.Errors!);
        })
            .MapToApiVersion(1)
            .AllowAnonymous();

        userRoutes.MapPost("login", async Task<Results<Ok<AccessTokenDTO>, ValidationProblem>> (LoginDTO loginDTO, IUserService userService) => 
        {
            ServiceResult<AccessTokenDTO> serviceResult = await userService.LoginAsync(loginDTO);

            return serviceResult.Success ? TypedResults.Ok(serviceResult.Data) : 
                                           TypedResults.ValidationProblem(serviceResult.Errors!);
        })
            .MapToApiVersion(1)
            .AllowAnonymous();

        userRoutes.MapPost("refresh", async Task<Results<Ok<AccessTokenDTO>, UnauthorizedHttpResult>> (HttpContext context, RefreshTokenDTO refreshToken, IUserService userService) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            ServiceResult<AccessTokenDTO> serviceResult = await userService.TokenRefresh(userId!, refreshToken.RefreshToken);

            return serviceResult.Success ? TypedResults.Ok(serviceResult.Data) :
                                           TypedResults.Unauthorized();
        })
            .MapToApiVersion(1)
            .RequireAuthorization("ExpiredToken");
    }
}
