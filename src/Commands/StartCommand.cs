namespace VasilyKengele.Commands;

public static class StartCommand
{
    public const string Name = "/start";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        if (!user.ReceiveWakeUps)
        {
            await usersRepository.AddAsync(user);
            var messageBuilder = new StringBuilder()
                .AppendLine($"Vasily Kengele welcomes you, commrade {user.Name}!");

            if (!user.TimeZoneSet)
            {
                messageBuilder.AppendLine($"Wake up with us at 5 o'clock.")
                    .Append($"To do that Vasily needs to know your timezone.");
            }
            else
            {
                messageBuilder.AppendLine($"You will now receive 5 o'clock wake ups from Vasily again.");
                user.ReceiveWakeUps = true;
                await usersRepository.UpdateAsync(user);
            }
            await botClient.SendTextMessageAsync(user.ChatId,
                text: messageBuilder.ToString(),
                cancellationToken: cancellationToken);

            if (!user.TimeZoneSet)
            {
                await TimeCommand.SendInlineKeyboardAsync(botClient, user, cancellationToken);
            }
            return;
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"You are all set, commrade {user.Name}.",
            cancellationToken: cancellationToken);
    }
}
