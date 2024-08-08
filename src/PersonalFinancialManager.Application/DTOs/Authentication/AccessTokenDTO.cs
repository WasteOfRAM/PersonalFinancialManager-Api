namespace PersonalFinancialManager.Application.DTOs.Authentication;

public class AccessTokenDTO
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}
