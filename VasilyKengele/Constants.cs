﻿namespace VasilyKengele;

public static class Constants
{
    public const int UpdateHour = 5;
    public const string BotToken = "TelegramBotToken";

    public const string StartCommand = "/start";
    public const string StopCommand = "/stop";
    public const string HelpCommand = "/help";

    public static IEnumerable<string> GetAllCommands()
    {
        yield return StartCommand;
        yield return StopCommand;
        yield return HelpCommand;
    }

    public static Dictionary<string, string> HelpStrings
    {
        get => new()
        {
            { StartCommand, $"Start receiving messages at {UpdateHour} o'clock." },
            { StopCommand, $"Stop receiving messages at {UpdateHour} o'clock." },
            { HelpCommand, "Display this help." },
        };
    }
}
