namespace VasilyKengele;

public static class Constants
{
    public const int UpdateHour = 12;
    public const string TelegramBotToken = "TelegramBotToken";
    public const string TwitterApiKey = "TwitterApiKey";
    public const string TwitterApiSecret = "TwitterApiSecret";
    public const string TwitterAccessToken = "TwitterAccessToken";
    public const string TwitterAccessTokenSecret = "TwitterAccessTokenSecret";

    public const string StartCommand = "/start";
    public const string StopCommand = "/stop";
    public const string UsersCountCommand = "/users_count";
    public const string AboutMeCommand = "/about_me";
    public const string DeleteMeCommand = "/delete_me";
    public const string TimeZoneSetCommand = "/time_set";
    public const string HelpCommand = "/help";

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

    public static Dictionary<string, string> HelpStrings
    {
        get => new()
        {
            { StartCommand, $"Start receiving messages at {UpdateHour} o'clock." },
            { StopCommand, $"Stop receiving messages at {UpdateHour} o'clock." },
            { UsersCountCommand, "Returns number of users currently waking up with us." },
            { AboutMeCommand, "Get JSON data stored about you by this bot." },
            { DeleteMeCommand, "Removes your data from out repository." },
            { TimeZoneSetCommand, $"Use this to tell Vasily your current time HOUR. He'll use it to calculate your timezone so your receive your wake up at your {Constants.UpdateHour} o'clock. (Message format is: '{Constants.TimeZoneSetCommand} number')." },
            { HelpCommand, "Display this help." },
        };
    }
}
