namespace VasilyKengele.Handlers;

/// <summary>
/// Class that handles Telegram bot messages.
/// </summary>
public class VKTelegramBotHandler
{
    private readonly VKBotUsersRepository _usersRepository;
    private readonly ILoggerAdapter _logger;
    private readonly IConfiguration _configuration;
    private readonly BotCommandFactory _commandFactory;

    public VKTelegramBotHandler(VKBotUsersRepository usersRepository,
                                ILoggerAdapter loggerAdapter,
                                IConfiguration configuration,
                                BotCommandFactory commandFactory)
    {
        _usersRepository = usersRepository;
        _logger = loggerAdapter;
        _configuration = configuration;
        _commandFactory = commandFactory;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient,
                                        Update update,
                                        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await ProcessTextMessageAsync(botClient, update, cancellationToken);
                break;
            case UpdateType.CallbackQuery:
                await ProcessInlineKeyboardButtonClickAsync(botClient, update, cancellationToken);
                break;
            default:
                _logger.Log(-1, "Unsupported Telegram Update received.");
                break;
        }
    }

    private async Task ProcessTextMessageAsync(ITelegramBotClient botClient,
                                               Update update,
                                               CancellationToken cancellationToken)
    {
        var messageText = update.Message!.Text;
        if (messageText is null)
        {
            return;
        }
        var chatId = update.Message.Chat.Id;
        var username = update.Message.Chat.Username;
        var fullname = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";
        (var user, var userExists) = await _usersRepository.GetAsync(chatId, fullname, username);

        try
        {
            var command = _commandFactory.MatchCommand(messageText);
            var parameters = new CommandParameters(botClient, _usersRepository, user, cancellationToken);

            if (command is TimeCommand or EmailSubscribeCommand && !userExists)
            {
                await HandleUnknownUserAsync(botClient, user, cancellationToken);
            }
            else
            {
                await command.ExecuteAsync(parameters);
            }
        }
        catch
        {
            await HandleUnknownCommandAsync(botClient, user.ChatId, cancellationToken);
        }
        _logger.Log(chatId, "Received a '{0}' message from {1} ({2}, {3}).", messageText, fullname, username, chatId);
    }

    private async Task ProcessInlineKeyboardButtonClickAsync(ITelegramBotClient botClient,
                                                             Update update,
                                                             CancellationToken cancellationToken)
    {
        var chatId = update.CallbackQuery!.From.Id;
        var username = update.CallbackQuery.From.Username;
        var fullname = $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}";
        (var user, var userExists) = await _usersRepository.GetAsync(chatId, fullname, username);

        var selectedHour = update.CallbackQuery.Data;
        if (userExists)
        {
            //Path 1: User exists and hour number argument from button is given.
            if (selectedHour is not null)
            {
                var parameters = new CommandParameters(botClient, _usersRepository, user, cancellationToken);
                await new TimeCommand(selectedHour).ExecuteAsync(parameters);
            }
            //Path 2: Something went wrong and button data is null.
            else
            {
                await HandleUnknownCommandAsync(botClient, user.ChatId, cancellationToken);
            }
        }
        //Path 3: User does not exist, prompt them to use /start to register first.
        else
        {
            await HandleUnknownUserAsync(botClient, user, cancellationToken);
        }
        _logger.Log(chatId, "Received /time response '{0}' message from {1} ({2}, {3}).", selectedHour, fullname, username, chatId);
    }

    private static async Task HandleUnknownCommandAsync(ITelegramBotClient botClient,
                                                        long chatId,
                                                        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(chatId,
            text: $"Vasily Kengele did not understand you!\nLearn available commands from /help.",
            cancellationToken: cancellationToken);
    }

    private static async Task HandleUnknownUserAsync(ITelegramBotClient botClient,
                                                     VKBotUserEntity user,
                                                     CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"Vasily Kengele does not recognize you.\nReactivate with /start.",
            cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient,
                                        Exception exception,
                                        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        _logger.Log(-1, errorMessage);
        return Task.CompletedTask;
    }
}
