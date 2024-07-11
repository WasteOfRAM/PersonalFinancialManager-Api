using Asp.Versioning;
using Asp.Versioning.Builder;
using PersonalFinancialManager.Application;
using PersonalFinancialManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddTokenService(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder routeGroup = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.Run();