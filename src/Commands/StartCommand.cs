namespace VasilyKengele.Commands;

/// <summary>
/// The bot entry command. Used to register the user.
/// </summary>
/// <remarks>Command string: <see cref="IVKBotCommand.Start"/></remarks>
public class StartCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        // Early return if user is already receiving wake ups.
        if (user.ReceiveWakeUps)
        {
            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: $"You are all set, {user.Name}.",
                cancellationToken: parameters.CancellationToken);
            return;
        }
        await parameters.UsersRepository.AddAsync(user);
        var messageBuilder = new StringBuilder()
            .AppendLine($"Vasily Kengele welcomes you, {user.Name}!");

        // Check if user has set up their timezone.
        await TimeZoneCheckAsync(messageBuilder, user, parameters.UsersRepository);

        // Send the built message to user.
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: messageBuilder.ToString(),
            cancellationToken: parameters.CancellationToken);

        // Also send inline keyboard if timezone is not set.
        if (!user.TimeZoneSet)
            await TimeCommand.SendInlineKeyboardAsync(parameters);
    }

    public async Task TimeZoneCheckAsync(StringBuilder str, VKBotUserEntity user, UsersRepositoryService repo)
    {
        if (user.TimeZoneSet)
        {
            str.AppendLine("You will now receive 5 o'clock wake ups from Vasily again.");
            user.ReceiveWakeUps = true;
            await repo.UpdateAsync(user);
            return;
        }
        str.AppendLine("Wake up with us at 5 o'clock.");
        str.Append("To do that Vasily needs to know your timezone.");
    }
}
