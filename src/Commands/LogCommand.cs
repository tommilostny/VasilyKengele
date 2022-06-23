namespace VasilyKengele.Commands;

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
        if (parameters.User.ChatId != MainChatId)
        {
            return;
        }
        await _logger.ReadLogsToBotAsync(_count, parameters);
    }
}
