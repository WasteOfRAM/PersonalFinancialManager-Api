namespace PersonalFinancialManager.WebApi.Endpoints;

using PersonalFinancialManager.Application.DTOs.User;
using PersonalFinancialManager.Application.Interfaces;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("user", async (CreateUserDTO userDTO, IUserService userService) =>
        {
            var user = await userService.CreateAsync(userDTO);

            return TypedResults.Ok(user);
        })
            .AllowAnonymous();
    }
}
