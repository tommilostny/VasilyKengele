namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM (<seealso cref="Constants.UpdateHour"/>) messages to registered users.
/// </summary>
public class VKTelegramBotInvocable : IInvocable
{
    private readonly string _botToken;
    private readonly VKBotUsersRepository _usersRepository;

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKTelegramBotInvocable(IConfiguration configuration, VKBotUsersRepository usersRepository)
    {
        _botToken = configuration[Constants.TelegramBotToken];
        _usersRepository = usersRepository;
    }

    /// <summary>
    /// This method is invoked at time scheduled by the Coravel library.
    /// <seealso cref="IInvocable.Invoke"/>
    /// </summary>
    public async Task Invoke()
    {
        var now = DateTime.UtcNow;
        var botClient = new TelegramBotClient(_botToken);

        foreach (var user in _usersRepository.GetAll())
        {
            if (user.Email is not null)
            {
                //Send email
            }
            if (!user.ReceiveWakeUps)
            {
                continue;
            }
            var userTime = now.AddHours(user.UtcDifference);
            if (userTime.Hour == Constants.UpdateHour)
            {
                var message = await botClient.SendTextMessageAsync(user.ChatId, $"Hey {user.Name}, it's {userTime}. Time to wake up!");
                Console.WriteLine($"Sent '{message.Text}' to: {message.Chat.Id}");
            }
        }
    }
}
