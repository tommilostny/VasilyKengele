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
    public const string TimeZoneSetCommand = "/time";
    public const string EmailSubscribeCommand = "/email";
    public const string EmailUnsubscribeCommand = "/email_delete";
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
        yield return EmailSubscribeCommand;
        yield return EmailUnsubscribeCommand;
        yield return HelpCommand;
    }

    /// <summary>
    /// Stores help strings for all available commands
    /// (accessible with ["command"]).
    /// </summary>
    public static ReadOnlyDictionary<string, string> HelpStrings { get; } = new(new Dictionary<string, string>
    {
        { StartCommand, $"Start receiving messages at {UpdateHour} o'clock." },
        { StopCommand, $"Stop receiving messages at {UpdateHour} o'clock." },
        { UsersCountCommand, "Returns number of users currently waking up with us." },
        { AboutMeCommand, "Get JSON data stored about you by this bot." },
        { DeleteMeCommand, "Removes your data from out repository." },
        { TimeZoneSetCommand, $"Use this to tell Vasily your current time <b>HOUR</b>. He'll use it to calculate your timezone so your receive your wake up at your correct {Constants.UpdateHour} o'clock time. (Message format for current time of 2:00 PM is: <code>{Constants.TimeZoneSetCommand} 14</code>)." },
        { EmailSubscribeCommand, $"Subscribe to the e-mail wake up notifications as well. Message format: <code>{EmailSubscribeCommand} person@email.com</code>." },
        { EmailUnsubscribeCommand, "Remove your email from our repository." },
        { HelpCommand, "Display this help." },
    });
}
