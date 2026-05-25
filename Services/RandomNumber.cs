using System;

namespace TgBot.Services;

public class RandomNumber
{
    private readonly Random _random = new();

    /// <summary>
    /// Парсит строку диапазона и генерирует случайное число.
    /// </summary>
    public string GenerateRandomNumberMessage(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
        {
            return "Вы не указали диапазон! Используйте формат: `/число [мин] [макс]`\nПример: `/число 1 100`";
        }

        // Разделяем строку по пробелам на отдельные аргументы
        string[] parts = inputText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Проверяем, что передано ровно два числа
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int min) ||
            !int.TryParse(parts[1], out int max))
        {
            return "Неверный формат. Пожалуйста, введите два целых числа через пробел.\nПример: `/число 10 50`";
        }

        // Защита: если пользователь перепутал местами минимум и максимум
        if (min > max)
        {
            // Меняем их местами с помощью кортежа
            (min, max) = (max, min);
        }

        // Генерируем число в диапазоне [min, max] включительно
        // В методе _random.Next верхняя граница эксклюзивна, поэтому прибавляем 1
        int randomNumber = _random.Next(min, max + 1);

        return $"🎲 Случайное число в диапазоне от **{min}** до **{max}**:\n\nРезультат: **{randomNumber}**";
    }
}