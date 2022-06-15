namespace VasilyKengele.Commands;

public static class HelpCommand
{
    public const string Name = "/help";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        var helpBuilder = new StringBuilder("<b>Available commands</b>:\n");
        foreach (var command in GetAllCommands())
        {
            helpBuilder.AppendLine($"{command}: {HelpStrings[command]}");
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: helpBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Iterates through all available bot commands.
    /// </summary>
    private static IEnumerable<string> GetAllCommands()
    {
        yield return StartCommand.Name;
        yield return StopCommand.Name;
        yield return TimeCommand.Name;
        yield return AboutMeCommand.Name;
        yield return DeleteMeCommand.Name;
        yield return UsersCountCommand.Name;
        yield return EmailSubscribeCommand.Name;
        yield return EmailUnsubscribeCommand.Name;
        yield return HelpCommand.Name;
    }

    /// <summary>
    /// Stores help strings for all available commands
    /// (accessible with ["command"]).
    /// </summary>
    private static ReadOnlyDictionary<string, string> HelpStrings { get; } = new(new Dictionary<string, string>
    {
        { StartCommand.Name, $"Start receiving messages at 5 o'clock." },
        { StopCommand.Name, $"Stop receiving messages at 5 o'clock." },
        { UsersCountCommand.Name, "Returns number of users currently waking up with us." },
        { AboutMeCommand.Name, "Get JSON data stored about you by this bot." },
        { DeleteMeCommand.Name, "Removes your data from out repository." },
        { TimeCommand.Name, $"Use this to tell Vasily your current time <b>HOUR</b>. He'll use it to calculate your timezone so your receive your wake up at your correct 5 o'clock time." },
        { EmailSubscribeCommand.Name, $"Subscribe to the e-mail wake up notifications as well. Message format: <code>{EmailSubscribeCommand.Name} person@email.com</code>." },
        { EmailUnsubscribeCommand.Name, "Remove your email from our repository." },
        { HelpCommand.Name, "Display this help." },
    });
}
