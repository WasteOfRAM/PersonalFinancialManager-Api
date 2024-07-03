namespace PersonalFinancialManager.Infrastructure.Interfaces;

using System.Security.Claims;

public interface ITokenService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);

    public string GenerateRefreshToken();
}
