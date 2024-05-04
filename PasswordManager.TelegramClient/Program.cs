
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager;
using PasswordManager.TelegramClient.Background;
using PasswordManager.TelegramClient.Commands;
using PasswordManager.TelegramClient.Commands.Handler;
using PasswordManager.TelegramClient.Data;
using PasswordManager.TelegramClient.Data.Repository;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);

var config = builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("secrets.json")
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddMemoryCache();
builder.Services.AddGrpcClient<PasswordStorageService.PasswordStorageServiceClient>(o => 
    o.Address = new Uri(config.GetRequiredSection("PasswordStorageService").Value!));
builder.Services.AddSingleton<IUserDataRepository, UserDataRepository>();
builder.Services.AddDbContext<TelegramClientContext>(o => o.UseNpgsql(config.GetConnectionString("postgresUserData")!));
builder.Services.AddSingleton(new TelegramBotClient(config.GetConnectionString("telegramBot")!));
builder.Services.AddSingleton<IUpdateHandler, TelegramMessageCommandHandler>();
builder.Services.AddSingleton<ITelegramCommandResolver, TelegramMessageCommandHandler>();
builder.Services.AddHostedService<TelegramUpdatesReceiver>();

var app = builder.Build();

app.Services.GetRequiredService<TelegramClientContext>().Database.Migrate();

app.Run();