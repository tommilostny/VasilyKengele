namespace VasilyKengele.Commands;

public class TimeCommand : IVKBotCommand
{
    private readonly string? _hourArg;

    public TimeCommand(string? hour = null)
    {
        _hourArg = hour;
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        if (_hourArg is null)
        {
            await SendInlineKeyboardAsync(parameters);
            return;
        }

        var user = parameters.User;
        if (_hourArg.All(c => char.IsDigit(c)))
        {
            var currentHour = Convert.ToInt32(_hourArg);
            if (currentHour >= 0 && currentHour < 24)
            {
                user.TimeZoneSet = user.ReceiveWakeUps = true;
                user.UtcDifference = currentHour - DateTime.UtcNow.Hour;
                await parameters.UsersRepository.UpdateAsync(user);

                var messageBuilder = new StringBuilder($"Congratulations {user.Name}, your timezone was set to UTC");
                if (user.UtcDifference >= 0)
                {
                    messageBuilder.Append('+');
                }
                messageBuilder.AppendLine($"{user.UtcDifference}!")
                    .AppendLine($"You will now receive 5 o'clock wake ups from Vasily Kengele in correct time.");

                await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                    text: messageBuilder.ToString(),
                    cancellationToken: parameters.CancellationToken);
                return;
            }
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"'{_hourArg}' is not a valid hour. Check the message format example with /help.",
            cancellationToken: parameters.CancellationToken);
    }

    public static async Task SendInlineKeyboardAsync(CommandParameters parameters)
    {
        //Setup keyboard dimensions.
        const byte buttonsCount = 24;
        const byte cols = 8;
        const byte rows = buttonsCount / cols;

        //Create buttons in the layout.
        var buttons = new List<InlineKeyboardButton[]>(rows);
        byte key = 0;
        for (byte row = 0; row < rows; row++)
        {
            var keyboardRow = new InlineKeyboardButton[cols];
            for (byte col = 0; col < cols; col++)
            {
                keyboardRow[col] = InlineKeyboardButton.WithCallbackData(key++.ToString());
            }
            buttons.Add(keyboardRow);
        }

        //Send the message with generated keyboard buttons.
        var inlineKeyboard = new InlineKeyboardMarkup(buttons);

        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: "Select your current time <b>HOUR</b>:",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard,
            cancellationToken: parameters.CancellationToken);
    }
}
