namespace VasilyKengele.Commands;

public static class TimeCommand
{
    public const string Name = "/time";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          string hourString,
                                          CancellationToken cancellationToken)
    {
        if (hourString.All(c => char.IsDigit(c)))
        {
            var currentHour = Convert.ToInt32(hourString);
            if (currentHour >= 0 && currentHour < 24)
            {
                user.TimeZoneSet = user.ReceiveWakeUps = true;
                user.UtcDifference = currentHour - DateTime.UtcNow.Hour;
                await usersRepository.UpdateAsync(user);

                var messageBuilder = new StringBuilder($"Congratulations {user.Name}, your timezone was set to UTC");
                if (user.UtcDifference >= 0)
                {
                    messageBuilder.Append('+');
                }
                messageBuilder.AppendLine($"{user.UtcDifference}!")
                    .AppendLine($"You will now receive 5 o'clock wake ups from Vasily Kengele in correct time.");

                await botClient.SendTextMessageAsync(user.ChatId,
                    text: messageBuilder.ToString(),
                    cancellationToken: cancellationToken);
                return;
            }
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"{hourString} is not a valid hour. Check the message format example with /help.",
            cancellationToken: cancellationToken);
    }

    public static async Task SendInlineKeyboardAsync(ITelegramBotClient botClient,
                                                     VKBotUserEntity user,
                                                     CancellationToken cancellationToken)
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

        await botClient.SendTextMessageAsync(user.ChatId,
            text: "Select your current time <b>HOUR</b>:",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}
