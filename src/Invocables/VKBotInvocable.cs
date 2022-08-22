namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM messages to registered users.
/// </summary>
public class VKBotInvocable : IInvocable
{
    private readonly ITelegramBotClient _botClient;
    private readonly VKBotUsersRepository _usersRepository;
    private readonly ILoggerAdapter _logger;
    private readonly IFluentEmailFactory? _fluentEmailFactory;

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKBotInvocable(ITelegramBotClient botClient,
                          VKBotUsersRepository usersRepository,
                          ILoggerAdapter loggerAdapter,
                          IFluentEmailFactory fluentEmailFactory,
                          IConfiguration configuration)
    {
        if (Convert.ToBoolean(configuration["Email:Enabled"]))
        {
            _fluentEmailFactory = fluentEmailFactory;
        }
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
                continue;

            var userTime = utcNow.AddHours(user.UtcDifference);

#if !DEBUG //Release mode: check if it is the correct 5 AM time. Debug mode: skip this check.
            if (userTime.Hour != 5)
                continue;
#endif
            //Create message and send it.
            var messageText = $"Hey {user.Name}, it's {userTime}. Time to wake up!";      
            await SendToTelegramBotAsync(user, messageText);
            await SendToEmailAsync(user, messageText);
        }
    }

    private async Task SendToTelegramBotAsync(VKBotUserEntity user, string messageText)
    {
        byte tries = 0;
        do try
        {
            var message = await _botClient.SendTextMessageAsync(user.ChatId, messageText);
            _logger.Log(user.ChatId, "Sent '{0}' to: {1}", message.Text, message.Chat.Id);
            break;
        }
        catch
        {
            tries++;
        }
        while (tries < 3);
        _logger.Log(user.ChatId, "Error sending message to Telegram chat ({0}).", user.ChatId);
    }

    private async Task SendToEmailAsync(VKBotUserEntity user, string messageText)
    {
        //Send message to e-mail if user has it setup.
        //Or skip if e-mail sending is not enabled in configuration.
        if (user.Email is null || _fluentEmailFactory is null)
            return;

        var email = _fluentEmailFactory.Create()
            .To(user.Email)
            .Subject("Wake up with Vasily Kengele")
            .Body(messageText);

        var emailResult = await email.SendAsync();
        if (emailResult.Successful)
        {
            _logger.Log(user.ChatId, "Sent e-mail '{0}' to {1}", messageText, user.Email);
            return;
        }
        _logger.Log(user.ChatId, "Unable to send e-mail to {0}", user.Email);
    }
}
