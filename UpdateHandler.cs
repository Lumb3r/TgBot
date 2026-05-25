using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Commands;

namespace TgBot;

public class UpdateHandler
{
    // Поле класса объявляется строго здесь, вне методов
    private readonly Countdown _countdown = new();

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Фильтруем и обрабатываем только текстовые сообщения
        if (update.Message is not { Text: { } messageText } message)
            return;

        long chatId = message.Chat.Id;
        Console.WriteLine($"Получено сообщение '{messageText}' в чате {chatId}.");

        // 1. Обработка команды-помощника
        if (messageText.StartsWith("/дата"))
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "Введите дату в формате **ДД.ММ.ГГГГ**, чтобы узнать сколько дней до нее осталось:",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
            return;
        }

        // 2. Проверяем, соответствует ли текст формату даты ХХ.ХХ.ХХХХ
        if (System.Text.RegularExpressions.Regex.IsMatch(messageText, @"^\d{2}\.\d{2}\.\d{4}$"))
        {
            // Передаем обработку в наш отдельный класс Countdown
            string responseText = _countdown.GetDaysRemainingMessage(messageText);

            await botClient.SendMessage(
                chatId: chatId,
                text: responseText,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка при работе бота: {exception.Message}");
        return Task.CompletedTask;
    }
}