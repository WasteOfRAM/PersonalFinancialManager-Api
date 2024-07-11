namespace PersonalFinancialManager.Core.Entities;

using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiration { get; set; }
}
