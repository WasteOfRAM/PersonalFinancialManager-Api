namespace PersonalFinancialManager.Application.DTOs.User;

public class CreateUserDTO
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}
