namespace VasilyKengele.Commands;

/// <summary>
/// Blueprint for a Telegram bot command that can be executed.
/// Also defines constants that are used as the commands in the chat bot.
/// </summary>
public interface IVKBotCommand
{
    /// <summary>
    /// Commands implemented behavior.
    /// </summary>
    /// <param name="parameters">Basic context of the Telegram bot that the command can use.</param>
    Task ExecuteAsync(CommandParameters parameters);

    const string Start = "/start";
    const string Stop = "/stop";
    const string UsersCount = "/users_count";
    const string AboutMe = "/about_me";
    const string DeleteMe = "/delete_me";
    const string Time = "/time";
    const string EmailSubscribe = "/email";
    const string EmailUnsubscribe = "/email_delete";
    const string Help = "/help";
    const string Log = "/log";
    const string Kick = "/kick";
}

/// <summary>
/// Wrapper for commonly used command parameters that maintain Telegram bots context.
/// </summary>
/// <param name="BotClient">Telegram bot client to send a response.</param>
/// <param name="UsersRepository">Users repository to read or update saved users.</param>
/// <param name="User">Current user that called the command.</param>
/// <param name="CancellationToken">Async task cancellation token.</param>
public record CommandParameters(ITelegramBotClient BotClient,
                                UsersRepositoryService UsersRepository,
                                VKBotUserEntity User,
                                CancellationToken CancellationToken);
