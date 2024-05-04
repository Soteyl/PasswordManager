using PasswordStorageService.Data;
using PasswordStorageService.Services.PasswordStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<PasswordStorageContext>();


var app = builder.Build();


app.MapGrpcService<PasswordStorageController>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();