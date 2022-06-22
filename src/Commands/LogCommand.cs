namespace VasilyKengele.Commands;

public class LogCommand : AuthenticatedCommandBase, IVKBotCommand
{
    private readonly IUserActionLoggerAdapter _logger;
    private readonly int _count;

    public LogCommand(IConfiguration configuration, IUserActionLoggerAdapter logger, int count) : base(configuration)
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
        foreach (var message in await _logger.ReadLogsAsync(_count))
        {
            await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId, message, ParseMode.Html);
        }
    }
}
