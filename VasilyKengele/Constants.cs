namespace VasilyKengele;

/// <summary>
/// Static class that contains constants used throughout the application.
/// </summary>
public static class Constants
{
    /// Wake up hour.
    public const int UpdateHour = 5;

    // Key in appsettings.json where the Telegram Bot API token is stored.
    public const string TelegramBotToken = "TelegramBotToken";

    // Available commands.
    public const string StartCommand = "/start";
    public const string StopCommand = "/stop";
    public const string UsersCountCommand = "/users_count";
    public const string AboutMeCommand = "/about_me";
    public const string DeleteMeCommand = "/delete_me";
    public const string TimeZoneSetCommand = "/time_set";
    public const string HelpCommand = "/help";

    /// <summary>
    /// Iterates through all available bot commands.
    /// </summary>
    public static IEnumerable<string> GetAllCommands()
    {
        yield return StartCommand;
        yield return StopCommand;
        yield return TimeZoneSetCommand;
        yield return AboutMeCommand;
        yield return DeleteMeCommand;
        yield return UsersCountCommand;
        yield return HelpCommand;
    }

    /// <summary>
    /// Stores help strings for all available commands
    /// (accessible with ["command"]).
    /// </summary>
    public static Dictionary<string, string> HelpStrings
    {
        get => new()
        {
            { StartCommand, $"Start receiving messages at {UpdateHour} o'clock." },
            { StopCommand, $"Stop receiving messages at {UpdateHour} o'clock." },
            { UsersCountCommand, "Returns number of users currently waking up with us." },
            { AboutMeCommand, "Get JSON data stored about you by this bot." },
            { DeleteMeCommand, "Removes your data from out repository." },
            { TimeZoneSetCommand, $"Use this to tell Vasily your current time HOUR. He'll use it to calculate your timezone so your receive your wake up at your {Constants.UpdateHour} o'clock. (Message format current time of 2:00 PM is: '{Constants.TimeZoneSetCommand} 14')." },
            { HelpCommand, "Display this help." },
        };
    }
}
