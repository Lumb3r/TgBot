using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgBot;

public class UpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Фильтруем и обрабатываем только текстовые сообщения
        if (update.Message is not { Text: { } messageText } message)
            return;

        long chatId = message.Chat.Id;
        Console.WriteLine($"Получено сообщение '{messageText}' в чате {chatId}.");

        // Ответ пользователю
        await botClient.SendMessage(
            chatId: chatId,
            text: $"Вы сказали: {messageText}",
            cancellationToken: cancellationToken
        );
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка при работе бота: {exception.Message}");
        return Task.CompletedTask;
    }
}