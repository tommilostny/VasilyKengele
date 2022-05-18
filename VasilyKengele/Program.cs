var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(provider => new TelegramBotClient(builder.Configuration[Constants.BotToken]));
builder.Services.AddSingleton<VKTelegramBotHandler>();
builder.Services.AddSingleton<VKTelegramChatIdsRepository>();
builder.Services.AddTransient<VKTelegramBotInvocable>();
builder.Services.AddScheduler();

// Build the ASP.NET Core application.
var app = builder.Build();

var botHandler = app.Services.GetService<VKTelegramBotHandler>();
if (botHandler is null)
{
    Console.Error.WriteLine($"Unable to create instance of {nameof(VKTelegramBotHandler)}.");
    return;
}
// Configure Telegram bot
var botClient = app.Services.GetService<TelegramBotClient>();
if (botClient is null)
{
    Console.Error.WriteLine($"Unable to create instance of {nameof(TelegramBotClient)}.");
    return;
}

var botReceiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message }
};
using var botCancellationTokenSource = new CancellationTokenSource();

botClient.StartReceiving(botHandler.HandleUpdateAsync,
                         botHandler.HandlePollingErrorAsync,
                         botReceiverOptions,
                         botCancellationTokenSource.Token
);
var me = await botClient.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//app.UseHttpsRedirection();

app.Services.UseScheduler(scheduler =>
{
#if DEBUG
    scheduler.Schedule<VKTelegramBotInvocable>().EveryFiveSeconds();
#else
    scheduler.Schedule<VKTelegramBotInvocable>().DailyAtHour(Constants.UpdateHour);
#endif
});

app.Run();

// Send cancellation request to stop bot
botCancellationTokenSource.Cancel();
