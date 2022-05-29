// Configure services.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(provider =>
{
    return new(builder.Configuration[Constants.TelegramBotToken]);
});
builder.Services.AddSingleton<VKTelegramBotHandler>();
builder.Services.AddSingleton<VKBotUsersRepository>();
builder.Services.AddTransient<VKBotInvocable>();
builder.Services.AddSingleton<IUserActionLoggerAdapter, InfluxDBLoggerAdapter>();

builder.Services.AddScheduler();
builder.Services
    .AddFluentEmail(builder.Configuration["Email:From"])
    .AddSmtpSender(new SmtpClient
    {
        Host = builder.Configuration["Email:SmtpHost"],
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(builder.Configuration["Email:SmtpUsername"], builder.Configuration["Email:SmtpPassword"]),
        Port = Convert.ToInt32(builder.Configuration["Email:SmtpPort"]),
        
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
    scheduler.Schedule<VKTelegramBotInvocable>().HourlyAt(0);
#endif
});

// Start the app.
app.Run("http://localhost:5234");

// Send cancellation request to stop bot
botCancellationTokenSource.Cancel();
