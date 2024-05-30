using Microsoft.EntityFrameworkCore;
using PasswordStorageService.Data;
using PasswordStorageService.Services.PasswordStorage;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .AddJsonFile("secrets.json")
    .AddEnvironmentVariables()
    .Build();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<PasswordStorageContext>(o => o.UseNpgsql(config.GetConnectionString("postgres")));


var app = builder.Build();

var passwordStorageContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<PasswordStorageContext>().Database;
passwordStorageContext.EnsureCreated();
passwordStorageContext.Migrate();

app.MapGrpcService<PasswordStorageController>();

app.Run();