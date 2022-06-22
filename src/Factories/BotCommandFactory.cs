namespace VasilyKengele.Factories;

public class BotCommandFactory
{
    private readonly IConfiguration _configuration;
    private readonly IUserActionLoggerAdapter _logger;

    public BotCommandFactory(IConfiguration configuration, IUserActionLoggerAdapter logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IVKBotCommand MatchCommand(string commandName)
    {
        return commandName switch
        {
            IVKBotCommand.Start => new StartCommand(),
            IVKBotCommand.Stop => new StopCommand(),
            IVKBotCommand.UsersCount => new UsersCountCommand(),
            IVKBotCommand.AboutMe => new AboutMeCommand(),
            IVKBotCommand.DeleteMe => new DeleteMeCommand(),
            IVKBotCommand.Time => new TimeCommand(),
            IVKBotCommand.EmailUnsubscribe => new EmailUnsubscribeCommand(),
            IVKBotCommand.Help => new HelpCommand(_configuration),

            var timeStr when commandName.StartsWith(IVKBotCommand.Time)
                => new TimeCommand(CommandArg(timeStr)),

            var emailStr when commandName.StartsWith(IVKBotCommand.EmailSubscribe)
                => new EmailSubscribeCommand(CommandArg(emailStr)),

            var logStr when commandName.StartsWith(IVKBotCommand.Log)
                => new LogCommand(_configuration, _logger, Convert.ToInt32(CommandArg(logStr))),

            _ => throw new InvalidOperationException()
        };
    }

    private static string CommandArg(string commandStr) => commandStr.Trim().Split(' ').Last();
}
