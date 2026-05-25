using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgBot.Commands;
using TgBot.Services; // Подключаем наши сервисы из папки Services

namespace TgBot.Handlers;

public class UpdateHandler
{
    // Инициализируем изолированные сервисы для каждой функции
    private readonly Countdown _countdown = new();
    private readonly ImageCompressor _imageCompressor = new();
    private readonly RandomNumber _randomService = new();

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // 1. ОБРАБОТКА ФОТОГРАФИЙ (Ухудшение качества картинок)
        if (update.Message is { MessageId: var messageId, Photo: { } photoSizes } message && update.Message.Type == MessageType.Photo)
        {
            long chatId = message.Chat.Id;
            var photo = photoSizes[^1]; // Безопасно берем самый большой размер оригинальной картинки

            Console.WriteLine($"[Photo] Получено фото от чата {chatId}. Размер: {photo.FileSize} байт.");

            using var memoryStream = new MemoryStream();

            // Скачиваем файл с серверов Telegram в оперативную память
            var file = await botClient.GetFile(photo.FileId, cancellationToken: cancellationToken);
            if (file.FilePath != null)
            {
                await botClient.DownloadFile(file.FilePath, memoryStream, cancellationToken);
            }
            memoryStream.Position = 0;

            // Сжимаем изображение через наш сервис
            using var lowQualityStream = _imageCompressor.CompressToLowQuality(memoryStream);

            // Отправляем сжатый результат обратно пользователю ответом на его сообщение
            await botClient.SendPhoto(
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
                                 "2. Шакалить картинки: просто отправь мне любое фото!\n" +
                                 "3. Генератор чисел: `/число [мин] [макс]` (например, `/рандом 1 100`)";

            await botClient.SendMessage(
                chatId: textChatId,
                text: welcomeText,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
            return;
        }

        // Команда /дата
        if (messageText.StartsWith("/дата", StringComparison.OrdinalIgnoreCase))
        {
            string dateInput = messageText.Substring("/дата".Length).Trim();

            if (string.IsNullOrWhiteSpace(dateInput))
            {
                await botClient.SendMessage(
                    chatId: textChatId,
                    text: "Вы не указали дату! Пример: `/дата 31.12.2026`",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                return;
            }

            // Передаем строку в сервис подсчета дат
            string responseText = _countdown.GetDaysRemainingMessage(dateInput);

            await botClient.SendMessage(
                chatId: textChatId,
                text: responseText,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
            return;
        }

        // Команда /рандом
        if (messageText.StartsWith("/число", StringComparison.OrdinalIgnoreCase))
        {
            // Извлекаем строку с числами после самой команды
            string rangeInput = messageText.Substring("/число".Length).Trim();

            // Передаем аргументы в сервис генерации случайных чисел
            string responseText = _randomService.GenerateRandomNumberMessage(rangeInput);

            await botClient.SendMessage(
                chatId: textChatId,
                text: responseText,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
            return;
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка при работе бота: {exception.Message}");
        return Task.CompletedTask;
    }
}