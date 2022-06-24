namespace VasilyKengele.Commands;

/// <summary>
/// Loads all available commands and provides helpful information for each of them.
/// </summary>
/// <remarks>Command string: <see cref="IVKBotCommand.Help"/></remarks>
public class HelpCommand : AuthenticatedCommandBase, IVKBotCommand
{
    public HelpCommand(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var helpBuilder = new StringBuilder("<b>Available commands</b>:\n");
        foreach (var command in GetAllCommands(parameters.User.ChatId))
        {
            helpBuilder.AppendLine($"{command}: {HelpStrings[command]}");
        }
        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: helpBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: parameters.CancellationToken);
    }

    /// <summary>
    /// Iterator through all command constants defined in <see cref="IVKBotCommand"/>.
    /// </summary>
    /// <param name="userChatId">Current user chat ID that is used to filter out commands available only to the main chat ID.</param>
    private IEnumerable<string> GetAllCommands(long userChatId)
    {
        yield return IVKBotCommand.Start;
        yield return IVKBotCommand.Stop;
        yield return IVKBotCommand.Time;
        yield return IVKBotCommand.AboutMe;
        yield return IVKBotCommand.DeleteMe;
        yield return IVKBotCommand.UsersCount;
        yield return IVKBotCommand.EmailSubscribe;
        yield return IVKBotCommand.EmailUnsubscribe;
        yield return IVKBotCommand.Help;
        if (userChatId == MainChatId)
        {
            yield return IVKBotCommand.Log;
        }
    }

    /// <summary>
    /// Dictionary that contains helpful information for all available commands.
    /// </summary>
    private static ReadOnlyDictionary<string, string> HelpStrings { get; } = new(new Dictionary<string, string>
    {
        { IVKBotCommand.Start, $"Start receiving messages at 5 o'clock." },
        { IVKBotCommand.Stop, $"Stop receiving messages at 5 o'clock." },
        { IVKBotCommand.Time, $"Use this to tell Vasily your current time <b>HOUR</b>. He'll use it to calculate your timezone so your receive your wake up at your correct 5 o'clock time." },
        { IVKBotCommand.UsersCount, "Returns number of users currently waking up with us." },
        { IVKBotCommand.AboutMe, "Get JSON data stored about you by this bot." },
        { IVKBotCommand.DeleteMe, "Removes your data from out repository." },
        { IVKBotCommand.EmailSubscribe, $"Subscribe to the e-mail wake up notifications as well. Message format: <code>{IVKBotCommand.EmailSubscribe} person@email.com</code>." },
        { IVKBotCommand.EmailUnsubscribe, "Remove your email from our repository." },
        { IVKBotCommand.Help, "Display this help." },
        { IVKBotCommand.Log, $"Load logs from InfluxDB. Message format to get last 20 logs: <code>{IVKBotCommand.Log} 20</code>" }
    });
}
