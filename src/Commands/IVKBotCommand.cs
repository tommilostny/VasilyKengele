namespace VasilyKengele.Commands;

public interface IVKBotCommand
{
    Task ExecuteAsync(CommandParameters parameters);

    static IVKBotCommand MatchCommandType(string commandName) => commandName switch
    {
        Start => new StartCommand(),
        Stop => new StopCommand(),
        UsersCount => new UsersCountCommand(),
        AboutMe => new AboutMeCommand(),
        DeleteMe => new DeleteMeCommand(),
        Time => new TimeCommand(),
        EmailUnsubscribe => new EmailUnsubscribeCommand(),
        Help => new HelpCommand(),

        var timeStr when commandName.StartsWith(Time)
            => new TimeCommand(timeStr.Trim().Split(' ').Last()),

        var emailStr when commandName.StartsWith(EmailSubscribe)
            => new EmailSubscribeCommand(emailStr.Trim().Split(' ').Last()),

        _ => throw new InvalidOperationException()
    };

    const string Start = "/start";
    const string Stop = "/stop";
    const string UsersCount = "/users_count";
    const string AboutMe = "/about_me";
    const string DeleteMe = "/delete_me";
    const string Time = "/time";
    const string EmailSubscribe = "/email";
    const string EmailUnsubscribe = "/email_delete";
    const string Help = "/help";
}

public record CommandParameters(ITelegramBotClient BotClient,
                                VKBotUsersRepository UsersRepository,
                                VKBotUserEntity User,
                                CancellationToken CancellationToken);
