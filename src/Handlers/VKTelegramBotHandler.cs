namespace VasilyKengele.Handlers;

/// <summary>
/// Class that handles Telegram bot messages.
/// </summary>
public class VKTelegramBotHandler
{
    private readonly VKBotUsersRepository _usersRepository;
    private readonly IUserActionLoggerAdapter _logger;

    /// <summary>
    /// Initializes <see cref="VKTelegramBotHandler"/> with user entities repository.
    /// </summary>
    /// <param name="usersRepository"><seealso cref="VKBotUsersRepository"/>.</param>
    public VKTelegramBotHandler(VKBotUsersRepository usersRepository, IUserActionLoggerAdapter loggerAdapter)
    {
        _usersRepository = usersRepository;
        _logger = loggerAdapter;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient,
                                        Update update,
                                        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                await ProcessInlineKeyboardButtonClickAsync(botClient, update, cancellationToken);
                break;
            case UpdateType.Message:
                await ProcessTextMessageAsync(botClient, update, cancellationToken);
                break;
            default:
                _logger.Log(-1, "Unsupported Telegram Update received.");
                break;
        }
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
            if (selectedHour is not null)
            {
                await TimeCommand.ExecuteAsync(botClient, _usersRepository, user, selectedHour, cancellationToken);
            }
            else await HandleUnknownCommandAsync(botClient, user.ChatId, cancellationToken);
        }
        else await HandleUnknownUserAsync(botClient, user, cancellationToken);

        _logger.Log(chatId, "Received /time response '{0}' message from {1} ({2}, {3}).", selectedHour, fullname, username, chatId);
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

        switch (messageText)
        {
            case StartCommand.Name:
                await StartCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case StopCommand.Name:
                await StopCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case UsersCountCommand.Name:
                await UsersCountCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case AboutMeCommand.Name:
                await AboutMeCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case DeleteMeCommand.Name:
                await DeleteMeCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case HelpCommand.Name:
                await HelpCommand.ExecuteAsync(botClient, user, cancellationToken);
                break;
            case EmailUnsubscribeCommand.Name:
                await EmailUnsubscribeCommand.ExecuteAsync(botClient, _usersRepository, user, cancellationToken);
                break;
            case TimeCommand.Name:
                await TimeCommand.SendInlineKeyboardAsync(botClient, user, cancellationToken);
                break;

            case var timeCommandStr when timeCommandStr.StartsWith(TimeCommand.Name):
                var hourStr = timeCommandStr.Trim().Split(' ').Last();
                if (userExists)
                {
                    await TimeCommand.ExecuteAsync(botClient, _usersRepository, user, hourStr, cancellationToken);
                    break;
                }
                await HandleUnknownUserAsync(botClient, user, cancellationToken);
                break;

            case var emailSubscribeStr when emailSubscribeStr.StartsWith(EmailSubscribeCommand.Name):
                var email = emailSubscribeStr.Trim().Split(' ').Last();
                if (userExists)
                {
                    await EmailSubscribeCommand.ExecuteAsync(botClient, _usersRepository, user, email, cancellationToken);
                    break;
                }
                await HandleUnknownUserAsync(botClient, user, cancellationToken);
                break;

            default:
                await HandleUnknownCommandAsync(botClient, user.ChatId, cancellationToken);
                break;
        }

        _logger.Log(chatId, "Received a '{0}' message from {1} ({2}, {3}).", messageText, fullname, username, chatId);
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
