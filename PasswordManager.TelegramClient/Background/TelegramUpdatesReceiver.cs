﻿using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace PasswordManager.TelegramClient.Background;

public class TelegramUpdatesReceiver: BackgroundService
{
    private readonly ITelegramBotClient _client;
    
    private readonly IUpdateHandler _updateHandler;

    public TelegramUpdatesReceiver(ITelegramBotClient client, IUpdateHandler updateHandler)
    {
        _client = client;
        _updateHandler = updateHandler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _client.ReceiveAsync(_updateHandler, cancellationToken: cancellationToken);
        }
    }
}