namespace VasilyKengele.Factories;

/// <summary>
/// Used to create an instance of <see cref="IVKBotCommand"/> based on chat bot text message input.
/// </summary>
public class BotCommandFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerAdapter _logger;

    public BotCommandFactory(IConfiguration configuration, ILoggerAdapter logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public IVKBotCommand MatchCommand(string command)
    {
        return command switch
        {
            IVKBotCommand.Start => new StartCommand(),
            IVKBotCommand.Stop => new StopCommand(),
            IVKBotCommand.UsersCount => new UsersCountCommand(_configuration),
            IVKBotCommand.AboutMe => new AboutMeCommand(),
            IVKBotCommand.DeleteMe => new DeleteMeCommand(),
            IVKBotCommand.Time => new TimeCommand(),
            IVKBotCommand.EmailUnsubscribe => new EmailUnsubscribeCommand(),
            IVKBotCommand.Help => new HelpCommand(_configuration),

            var timeStr when command.StartsWith(IVKBotCommand.Time)
                => new TimeCommand(CommandArg(timeStr)),

            var emailStr when command.StartsWith(IVKBotCommand.EmailSubscribe)
                => new EmailSubscribeCommand(CommandArg(emailStr)),

            var logStr when command.StartsWith(IVKBotCommand.Log)
                => new LogCommand(_configuration, _logger, Convert.ToInt32(CommandArg(logStr))),

            _ => throw new InvalidOperationException()
        };
    }

    private static string CommandArg(string commandStr) => commandStr.Trim().Split(' ').Last();
}
