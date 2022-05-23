namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM (<seealso cref="Constants.UpdateHour"/>) messages to registered users.
/// </summary>
public class VKTelegramBotInvocable : IInvocable
{
    private readonly TelegramBotClient _botClient;
    private readonly VKBotUsersRepository _usersRepository;
    private readonly IUserActionLoggerAdapter _logger;

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKTelegramBotInvocable(TelegramBotClient botClient,
                                  VKBotUsersRepository usersRepository,
                                  IUserActionLoggerAdapter loggerAdapter)
    {
        _botClient = botClient;
        _usersRepository = usersRepository;
        _logger = loggerAdapter;
    }

    /// <summary>
    /// This method is invoked at time scheduled by the Coravel library.
    /// <seealso cref="IInvocable.Invoke"/>
    /// </summary>
    public async Task Invoke()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var user in _usersRepository.GetAll())
        {
            if (!user.ReceiveWakeUps)
            {
                continue;
            }
            var userTime = utcNow.AddHours(user.UtcDifference);
#if !DEBUG
            if (userTime.Hour == Constants.UpdateHour)
#endif
            {
                var messageText = $"Hey {user.Name}, it's {userTime}. Time to wake up!";

                var message = await _botClient.SendTextMessageAsync(user.ChatId, messageText);
                _logger.Log(user.ChatId, "Sent '{0}' to: {1}", message.Text, message.Chat.Id);
                
                if (user.Email is not null)
                {
                    //Send email
                    _logger.Log(user.ChatId, "Sent e-mail '{0}' to: {1}", messageText, user.Email);
                }
            }
        }
    }
}
