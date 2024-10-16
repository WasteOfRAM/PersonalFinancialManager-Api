﻿namespace PersonalFinancialManager.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Entities;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Account>()
            .ToTable(tb => tb.HasCheckConstraint("CK_TotalNonNegative", "[Total] >= 0.0"));

        base.OnModelCreating(builder);
    }
}
