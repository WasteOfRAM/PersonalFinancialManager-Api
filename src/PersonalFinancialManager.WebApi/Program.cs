using PersonalFinancialManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddTokenService(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();