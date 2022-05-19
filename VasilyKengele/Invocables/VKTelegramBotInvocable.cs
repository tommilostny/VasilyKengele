namespace VasilyKengele.Invocables;

public class VKTelegramBotInvocable : IInvocable
{
    private readonly string _botToken;
    private readonly VKBotUsersRepository _usersRepository;

    public VKTelegramBotInvocable(IConfiguration configuration, VKBotUsersRepository usersRepository)
    {
        _botToken = configuration[Constants.TelegramBotToken];
        _usersRepository = usersRepository;
    }

    public async Task Invoke()
    {
        var now = DateTime.UtcNow;
        var botClient = new TelegramBotClient(_botToken);

        foreach (var user in _usersRepository.GetAll())
        {
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
