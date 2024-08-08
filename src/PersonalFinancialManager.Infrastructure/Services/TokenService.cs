namespace PersonalFinancialManager.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinancialManager.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        // TODO: Change the expires time for something longer after testing
        var token = new JwtSecurityToken
        (
            issuer: configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[256];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
