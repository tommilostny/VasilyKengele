// Configure services.
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    services.AddSingleton<ILoggerAdapter, InfluxDBLoggerAdapter>();

    services.AddSingleton<VKTelegramBotHandler>();
    services.AddSingleton<VKBotUsersRepository>();
    services.AddTransient<VKBotInvocable>();
    services.AddSingleton<BotCommandFactory>();

    services.AddScheduler();

    services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
    {
        return new(context.Configuration["TelegramBotToken"] ?? string.Empty);
    });

    services.AddFluentEmail(context.Configuration["Email:From"])
        .AddSmtpSender(new SmtpClient
        {
            Host = context.Configuration["Email:Smtp:Host"] ?? string.Empty,
            Port = Convert.ToInt32(context.Configuration["Email:Smtp:Port"]),
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(context.Configuration["Email:Smtp:Username"],
                                                context.Configuration["Email:Smtp:Password"])
        });

    services.AddOpenAIService(settings =>
    {
        settings.ApiKey = context.Configuration["OpenAI:ApiKey"] ?? string.Empty;
    });
});

// Build the .NET application.
var app = builder.Build();

// Configure Telegram bot.
var botHandler = app.Services.GetService<VKTelegramBotHandler>();
if (botHandler is null)
{
    Console.Error.WriteLine($"Unable to create instance of {nameof(VKTelegramBotHandler)}.");
    return;
}
var botClient = app.Services.GetService<ITelegramBotClient>();
if (botClient is null)
{
    Console.Error.WriteLine($"Unable to create instance of {nameof(ITelegramBotClient)}.");
    return;
}
var botReceiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
};
using var botCTS = new CancellationTokenSource();

// Start Telegram bot listening.
botClient.StartReceiving(botHandler.HandleUpdateAsync,
                         botHandler.HandlePollingErrorAsync,
                         botReceiverOptions,
                         botCTS.Token
);
var me = await botClient.GetMeAsync();
Console.WriteLine($"Start of Telegram Bot listening for @{me.Username}");

// Schedule messages to go out at 5:00 AM (based on users timezone).
app.Services.UseScheduler(scheduler =>
{
#if DEBUG
    scheduler.Schedule<VKBotInvocable>().EveryTenSeconds();
#else
    scheduler.Schedule<VKBotInvocable>().HourlyAt(0);
#endif
});

// Start the app.
await app.RunAsync(botCTS.Token);

// Send cancellation request to stop bot.
botCTS.Cancel();
