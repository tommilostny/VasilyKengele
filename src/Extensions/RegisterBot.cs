namespace VasilyKengele.Extensions;

public static class RegisterBot
{
    public static async Task UseVKBotAsync(this IServiceProvider services, CancellationToken ct)
    {
        var botHandler = services.GetService<TelegramHandlerService>();
        if (botHandler is null)
        {
            Console.Error.WriteLine($"Unable to create instance of {nameof(TelegramHandlerService)}.");
            return;
        }
        var botClient = services.GetService<ITelegramBotClient>();
        if (botClient is null)
        {
            Console.Error.WriteLine($"Unable to create instance of {nameof(ITelegramBotClient)}.");
            return;
        }
        var botReceiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
        };

        // Start Telegram bot listening.
        botClient.StartReceiving(botHandler.HandleUpdateAsync,
                                botHandler.HandlePollingErrorAsync,
                                botReceiverOptions, ct);
        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Start of Telegram Bot listening for @{me.Username}");
    }
}
