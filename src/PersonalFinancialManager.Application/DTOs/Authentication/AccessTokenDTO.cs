namespace PersonalFinancialManager.Application.DTOs.Authentication;

public class AccessTokenDTO
{
    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}
