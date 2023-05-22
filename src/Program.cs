// Configure services.
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    VKConfiguration config;
    try // Add secrets configuration.
    {
        config = services.AddVKSecrets();
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
        return;
    }
    // Add logging.
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });
    services.AddSingleton<ILoggerAdapter, InfluxDBLoggerAdapter>();
    // Add Vasiliy Kengele services.
    services.AddSingleton<BotCommandService>();
    services.AddSingleton<TelegramHandlerService>();
    services.AddSingleton<UsersRepositoryService>();
    services.AddSingleton<VKOpenAIService>();
    services.AddSingleton<VKEmailService>();
    services.AddTransient<VKBotInvocable>();
    // Add Coravel scheduler.
    services.AddScheduler();
    // Add Telegram bot.
    services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
    {
        return new(config.TelegramBotToken);
    });
    // Add FluentEmail library.
    services.AddFluentEmail(config.Email.From, "Vasily Kengele")
        .AddSmtpSender(new SmtpClient
        {
            Host = config.Email.Smtp.Host,
            Port = config.Email.Smtp.Port,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(config.Email.Smtp.Username,
                                                config.Email.Smtp.Password)
        });
    // Add OpenAI API library.
    services.AddOpenAIService(settings =>
    {
        settings.ApiKey = config.OpenAI.ApiKey;
    });
});

// Build the .NET application.
var app = builder.Build();

// Configure Telegram bot.
using var botCTS = new CancellationTokenSource();
await app.Services.UseVKBotAsync(botCTS.Token);

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
