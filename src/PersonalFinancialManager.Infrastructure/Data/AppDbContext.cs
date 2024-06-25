namespace PersonalFinancialManager.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Entities;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
