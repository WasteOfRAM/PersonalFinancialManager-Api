namespace PersonalFinancialManager.UnitTests.ServiceTests;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinancialManager.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenServiceTests
{
    private readonly IConfiguration configuration;
    private readonly TokenService tokenService;

    public TokenServiceTests()
    {
        configuration = Substitute.For<IConfiguration>();

        configuration["Jwt:Key"].Returns("erhgrRTGsadfef545erdgdfgr6tDFHGR4653fgdggsdf");
        configuration["Jwt:Issuer"].Returns("testIssuer");
        configuration["Jwt:Audience"].Returns("testAudience");
        configuration["Jwt:TokenExpirationInMinutes"].Returns("10");
        configuration["Jwt:RefreshTokenExpirationInMinutes"].Returns("10");

        tokenService = new TokenService(configuration);
    }

    [Fact]
    public void GenerateAccessToken_Generates_Valid_JWT()
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var fixture = new Fixture();

        var claims = fixture.Create<ClaimsPrincipal>().Claims;

        var jwtResult = tokenService.GenerateAccessToken(claims);

        tokenHandler.ValidateToken(jwtResult, tokenValidationParameters, out SecurityToken validatedToken);

        Assert.Multiple(() =>
        {
            Assert.NotNull(validatedToken);
            Assert.NotNull(jwtResult);
        });
    }

    [Fact]
    public void GenerateRefreshToken_Generates_Valid_Base64String()
    {
        var result = tokenService.GenerateRefreshToken();

        var data = Convert.FromBase64String(result);

        Assert.Equal(256, data.Length);
    }
}
