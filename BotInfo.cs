using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using TgBot.Handlers;

namespace TgBot;
public class BotInfo
{
    private readonly TelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private CancellationTokenSource? _cts;

    public BotInfo(string token)
    {
        _botClient = new TelegramBotClient(token);
        _updateHandler = new UpdateHandler();
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();

        _botClient.StartReceiving(
            updateHandler: _updateHandler.HandleUpdateAsync,
            errorHandler: _updateHandler.HandlePollingErrorAsync,
            cancellationToken: _cts.Token
        );

        var me = await _botClient.GetMe();
        Console.WriteLine($"Бот @{me.Username} успешно запущен!");
    }

    public void Stop()
    {
        _cts?.Cancel();
        Console.WriteLine("Бот остановлен.");
    }
}