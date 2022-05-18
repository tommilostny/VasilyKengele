namespace VasilyKengele.Handlers;

public class VKTelegramBotHandler
{
    private readonly VKTelegramChatIdsRepository _chatIdsRepository;

    public VKTelegramBotHandler(VKTelegramChatIdsRepository chatIdsRepository)
    {
        _chatIdsRepository = chatIdsRepository;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient,
                                        Update update,
                                        CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        // And only process text messages
        if (update.Type != UpdateType.Message || update.Message!.Type != MessageType.Text)
        {
            return;
        }
        var chatId = update.Message.Chat.Id;
        var messageText = update.Message.Text;
        var username = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        switch (messageText)
        {
            case Constants.StartCommand:
                await ExecuteStartCommandAsync(botClient, chatId, username, cancellationToken);
                break;
            case Constants.StopCommand:
                await ExecuteStopCommandAsync(botClient, chatId, username, cancellationToken);
                break;
            case Constants.HelpCommand:
            default:
                await ExecuteHelpCommand(botClient, chatId, cancellationToken);
                break;
        }
    }

    private async Task ExecuteStartCommandAsync(ITelegramBotClient botClient,
                                                long chatId,
                                                string username,
                                                CancellationToken cancellationToken)
    {
        await _chatIdsRepository.AddAsync(chatId);
        await botClient.SendTextMessageAsync(chatId,
            text: $"Vasily Kengele welcomes you, commrade {username}!\nWake up with us at {Constants.UpdateHour} o'clock.",
            cancellationToken: cancellationToken);
    }

    private async Task ExecuteStopCommandAsync(ITelegramBotClient botClient,
                                               long chatId,
                                               string username,
                                               CancellationToken cancellationToken)
    {
        await _chatIdsRepository.RemoveAsync(chatId);
        await botClient.SendTextMessageAsync(chatId,
            text: $"Goodbye, commrade {username}!\nVasily Kengele is sad to see you leave.",
            cancellationToken: cancellationToken);
    }

    private static async Task ExecuteHelpCommand(ITelegramBotClient botClient,
                                                 long chatId,
                                                 CancellationToken cancellationToken)
    {
        var helpBuilder = new StringBuilder("Available commands:\n");
        foreach (var command in Constants.GetAllCommands())
        {
            helpBuilder.AppendLine($"{command}: {Constants.HelpStrings[command]}");
        }
        await botClient.SendTextMessageAsync(chatId,
            text: helpBuilder.ToString(),
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
        Console.Error.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
