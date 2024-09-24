﻿namespace PersonalFinancialManager.IntegrationTests;

using Testcontainers.MsSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using PersonalFinancialManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly MsSqlContainer msSqlContainer = new MsSqlBuilder().WithName("MSSQLTests").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => 
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            msSqlContainer.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            // Testcontainers.MsSql does NOT support custom Database name in the connection string because if the database does not exist the connection will fail.
            // This is possible only because EF Core can create the database and needed because EF Core can perform operations on the database that are restricted by the master database.
            var connectionString = msSqlContainer.GetConnectionString().Replace("master", "MSSQLTestDB");

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await msSqlContainer.StopAsync();

        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}