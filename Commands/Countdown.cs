using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TgBot.Commands
{
    internal class Countdown
    {
        private const string DateFormat = "dd.MM.yyyy";

        /// <summary>
        /// Парсит строку и возвращает количество дней до указанной даты.
        /// </summary>
        /// <param name="inputText">Строка от пользователя (например, "31.12.2026")</param>
        /// <returns>Строка с результатом для отправки пользователю</returns>
        public string GetDaysRemainingMessage(string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return "Пожалуйста, отправьте корректную дату.";
            }

            // Проверяем строгое соответствие формату ДД.ММ.ГГГГ
            if (!DateTime.TryParseExact(inputText.Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime targetDate))
            {
                return "Неверный формат даты. Пожалуйста, используйте формат **ДД.ММ.ГГГГ** (например, `31.12.2026`).";
            }

            DateTime today = DateTime.Today;

            if (targetDate < today)
            {
                return "Эта дата уже в прошлом! Пожалуйста, введите будущую дату.";
            }

            int daysLeft = (targetDate - today).Days;

            return daysLeft switch
            {
                0 => "Этот день уже сегодня! 🎉",
                1 => "Остался всего 1 день! ⏳",
                _ => $"До заданной даты осталось дней: **{daysLeft}** 📅"
            };
        }
    }
}
