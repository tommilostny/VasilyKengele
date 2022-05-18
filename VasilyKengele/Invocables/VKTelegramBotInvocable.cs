namespace VasilyKengele.Invocables;

public class VKTelegramBotInvocable : IInvocable
{
    private readonly string _botToken;
    private readonly VKTelegramChatIdsRepository _chatIdsRepository;

    public VKTelegramBotInvocable(IConfiguration configuration, VKTelegramChatIdsRepository chatIdsRepository)
    {
        _botToken = configuration[Constants.BotToken];
        _chatIdsRepository = chatIdsRepository;
    }

    public async Task Invoke()
    {
        var botClient = new TelegramBotClient(_botToken);
        foreach (var chatId in _chatIdsRepository.GetAll())
        {
            var message = await botClient.SendTextMessageAsync(chatId, $"Hey, it's {DateTime.Now}, wake up!");
            Console.WriteLine($"Sent '{message.Text}' to: {message.Chat.Id}");
        }
    }
}
