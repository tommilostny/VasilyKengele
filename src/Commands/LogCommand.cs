namespace VasilyKengele.Commands;

/// <summary>
/// Allows the bot owner to read stored logs directly from the chat bot.
/// Used by the user specified by the <see cref="AuthenticatedCommandBase.MainChatId"/>.
/// </summary>
/// <remarks>Command string: <see cref="IVKBotCommand.Log"/></remarks>
public class LogCommand : AuthenticatedCommandBase, IVKBotCommand
{
    private readonly ILoggerAdapter _logger;
    private readonly int _count;

    public LogCommand(IConfiguration configuration, ILoggerAdapter logger, int count) : base(configuration)
    {
        _logger = logger;
        _count = count;
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        if (parameters.User.ChatId == MainChatId)
        {
            await _logger.ReadLogsToBotAsync(_count, parameters);
        }
    }
}
