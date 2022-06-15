namespace VasilyKengele.Commands;

public class HelpCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var helpBuilder = new StringBuilder("<b>Available commands</b>:\n");
        foreach (var command in GetAllCommands())
        {
            helpBuilder.AppendLine($"{command}: {HelpStrings[command]}");
        }
        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: helpBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: parameters.CancellationToken);
    }

    private static IEnumerable<string> GetAllCommands()
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
    }

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
    });
}
