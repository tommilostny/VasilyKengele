namespace VasilyKengele.Commands;

public class StartCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        if (!user.ReceiveWakeUps)
        {
            await parameters.UsersRepository.AddAsync(user);
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
                await parameters.UsersRepository.UpdateAsync(user);
            }
            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: messageBuilder.ToString(),
                cancellationToken: parameters.CancellationToken);

            if (!user.TimeZoneSet)
            {
                await TimeCommand.SendInlineKeyboardAsync(parameters);
            }
            return;
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"You are all set, commrade {user.Name}.",
            cancellationToken: parameters.CancellationToken);
    }
}
