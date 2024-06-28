using Asp.Versioning;
using Asp.Versioning.Builder;
using PersonalFinancialManager.Infrastructure;
using PersonalFinancialManager.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddTokenService(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder routeGroup = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .RequireAuthorization();

routeGroup.MapUserEndpoints();

app.Run();