
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PasswordManager;
using PasswordManager.TelegramClient.Background;
using PasswordManager.TelegramClient.Common.Extensions;
using PasswordManager.TelegramClient.Data;
using PasswordManager.TelegramClient.Data.Repository;
using PasswordManager.TelegramClient.FormRegistrations.Handler;
using PasswordManager.TelegramClient.Messenger;
using Serilog;
using Serilog.Core;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TelegramMessengerClient = PasswordManager.TelegramClient.Messenger.TelegramMessengerClient;

var builder = Host.CreateApplicationBuilder(args);

var config = builder.Configuration
    .AddJsonFile("secrets.json")
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddMemoryCache();
builder.Services.AddGrpcClient<PasswordStorageService.PasswordStorageServiceClient>(o => 
    o.Address = new Uri(config.GetRequiredSection("PasswordStorageService").Value!));
builder.Services.AddSingleton<IUserDataRepository, UserDataRepository>();
builder.Services.AddDbContext<TelegramClientContext>(o => o.UseNpgsql(config.GetConnectionString("postgresUserData")!));
builder.Services.AddDbContextFactory<TelegramClientContext>();
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config.GetConnectionString("telegramBot")!));
builder.Services.AddSingleton<IMessengerClient, TelegramMessengerClient>();

builder.Services.AddAllFormRegistrations(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<TelegramFormMessageHandler>();
    
builder.Services.AddSingleton<IUpdateHandler, TelegramMessageCommandHandler>();
builder.Services.AddHostedService<TelegramUpdatesReceiver>();

var app = builder.Build();

app.Services.GetRequiredService<TelegramClientContext>().Database.Migrate();

app.Run();