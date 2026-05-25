using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Handlers;
using TgBot.Commands;
using TgBot.Services;

namespace TgBot.Handlers;

public class UpdateHandler
{
    private readonly Countdown _countdown = new();
    private readonly ImageCompressor _imageCompressor = new();

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // 1. ОБРАБОТКА ФОТОГРАФИЙ
        if (update.Message is { MessageId: var messageId } message && update.Message.Type == MessageType.Photo)
        {
            long chatId = message.Chat.Id;
            var photo = message.Photo[^1]; // Берем самый большой размер

            Console.WriteLine($"[Photo] Получено фото от чата {chatId}.");

            using var memoryStream = new MemoryStream();
            await botClient.GetInfoAndDownloadFileAsync(photo.FileId, memoryStream, cancellationToken);
            memoryStream.Position = 0;

            // Вызываем изолированный сервис для ухудшения качества
            using var lowQualityStream = _imageCompressor.CompressToLowQuality(memoryStream);

            await botClient.SendPhotoAsync(
                chatId: chatId,
                photo: InputFile.FromStream(lowQualityStream, "shakal.jpg"),
                caption: "Держи своё ухудшенное фото! 😎 👌",
                replyParameters: new ReplyParameters { MessageId = messageId },
                cancellationToken: cancellationToken
            );
            return;
        }

        // 2. ОБРАБОТКА ТЕКСТОВЫХ СООБЩЕНИЙ
        if (update.Message is not { Text: { } messageText } textMessage)
            return;

        long textChatId = textMessage.Chat.Id;
        Console.WriteLine($"[Text] Получено сообщение '{messageText}' в чате {textChatId}.");

        // Команда /start
        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            string welcomeText = "Привет! 👋 Я умею:\n" +
                                 "1. Считать дни: `/дата ДД.ММ.ГГГГ`\n" +
                                 "2. Шакалить картинки: просто отправь мне любое фото!";

            await botClient.SendMessage(chatId: textChatId, text: welcomeText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            return;
        }

        // Команда /дата
        if (messageText.StartsWith("/дата", StringComparison.OrdinalIgnoreCase))
        {
            string dateInput = messageText.Substring("/дата".Length).Trim();

            if (string.IsNullOrWhiteSpace(dateInput))
            {
                await botClient.SendMessage(chatId: textChatId, text: "Вы не указали дату! Пример: `/дата 31.12.2026`", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                return;
            }

            // Вызываем изолированный сервис для расчета дней
            string responseText = _countdown.GetDaysRemainingMessage(dateInput);
            await botClient.SendMessage(chatId: textChatId, text: responseText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка при работе бота: {exception.Message}");
        return Task.CompletedTask;
    }
}