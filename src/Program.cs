// Loading secrets work-around using a second temporary configuration.
var configuration = new ConfigurationBuilder()
                .AddUserSecrets<VKConfiguration>()
                .Build();

var services = new ServiceCollection()
    .Configure<VKConfiguration>(configuration.GetSection(nameof(VKConfiguration)))
    .AddOptions()
    .BuildServiceProvider();

var myConf = services.GetService<IOptions<VKConfiguration>>()?.Value ?? throw new Exception("Secrets not loaded...");

// Configure services.
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton(myConf);

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
        return new(myConf.TelegramBotToken);
    });

    services.AddFluentEmail(myConf.Email.From, "Vasily Kengele")
        .AddSmtpSender(new SmtpClient
        {
            Host = myConf.Email.Smtp.Host,
            Port = myConf.Email.Smtp.Port,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(myConf.Email.Smtp.Username, myConf.Email.Smtp.Password)
        });

    services.AddOpenAIService(settings =>
    {
        settings.ApiKey = myConf.OpenAI.ApiKey;
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
