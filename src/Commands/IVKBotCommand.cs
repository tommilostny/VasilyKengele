namespace VasilyKengele.Commands;

public interface IVKBotCommand
{
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
}

public record CommandParameters(ITelegramBotClient BotClient,
                                VKBotUsersRepository UsersRepository,
                                VKBotUserEntity User,
                                CancellationToken CancellationToken);
