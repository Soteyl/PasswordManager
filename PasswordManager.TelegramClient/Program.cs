
using System.Reflection;
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
using PasswordManager.TelegramClient.Extensions;
using PasswordManager.TelegramClient.Telegram;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramMessengerClient = PasswordManager.TelegramClient.Messenger.TelegramMessengerClient;

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
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config.GetConnectionString("telegramBot")!));
builder.Services.AddSingleton<IMessengerClient, TelegramMessengerClient>();

builder.Services.AddAllFormRegistrations(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<TelegramFormMessageHandler>();
    
builder.Services.AddSingleton<IUpdateHandler, TelegramMessageCommandHandler>();
builder.Services.AddSingleton<ITelegramCommandResolver, TelegramMessageCommandResolver>();
builder.Services.AddHostedService<TelegramUpdatesReceiver>();

var app = builder.Build();

app.Services.GetRequiredService<TelegramClientContext>().Database.Migrate();

app.Run();