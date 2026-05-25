using System;
using System.Threading.Tasks;
using TgBot.Handlers;

namespace TgBot;

class Program
{
    private static string Token = "8909494876:AAFRK4aIM9fqleoBTxQJf3Tfmd7MScMMjGI";

    static async Task Main(string[] args)
    {
        var bot = new BotInfo(Token);

        await bot.StartAsync();

        Console.WriteLine("Нажмите ESC для выхода из программы.");
        while (Console.ReadKey(true).Key != ConsoleKey.Escape){}
        bot.Stop();
    }
}