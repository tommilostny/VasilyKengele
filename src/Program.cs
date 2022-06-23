// Configure services.
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<ILoggerAdapter, InfluxDBLoggerAdapter>();

builder.Services.AddSingleton<VKTelegramBotHandler>();
builder.Services.AddSingleton<VKBotUsersRepository>();
builder.Services.AddTransient<VKBotInvocable>();
builder.Services.AddSingleton<BotCommandFactory>();

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
{
    return new(builder.Configuration["TelegramBotToken"]);
});

builder.Services.AddScheduler();

builder.Services
    .AddFluentEmail(builder.Configuration["Email:From"])
    .AddSmtpSender(new SmtpClient
    {
        Host = builder.Configuration["Email:Smtp:Host"],
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(builder.Configuration["Email:Smtp:Username"], builder.Configuration["Email:Smtp:Password"]),
        Port = Convert.ToInt32(builder.Configuration["Email:Smtp:Port"])
    });

// Build the ASP.NET Core application.
var app = builder.Build();

// Configure Telegram bot
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
using var botCancellationTokenSource = new CancellationTokenSource();

// Start Telegram bot listening.
botClient.StartReceiving(botHandler.HandleUpdateAsync,
                         botHandler.HandlePollingErrorAsync,
                         botReceiverOptions,
                         botCancellationTokenSource.Token
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
app.Run("http://localhost:5234");

// Send cancellation request to stop bot
botCancellationTokenSource.Cancel();
