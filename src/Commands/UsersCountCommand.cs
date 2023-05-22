namespace VasilyKengele.Commands;

/// <summary>
/// Loads user statistics from the repository so the users can see how the bot is doing.
/// Gives the amount of users that currently receive and don't receive their notifications at 5 AM.
/// </summary>
/// <remarks>Command string: <see cref="IVKBotCommand.UsersCount"/></remarks>
public class UsersCountCommand : AuthenticatedCommandBase, IVKBotCommand
{
    public UsersCountCommand(VKConfiguration configuration) : base(configuration)
    {
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var users = parameters.UsersRepository.GetAll();
        var wakingUp = users.Count(u => u.ReceiveWakeUps);
        var notWakingUp = users.Count - wakingUp;

        var messageBuilder = new StringBuilder($"Right now <b>{wakingUp}</b> user")
            .Append(wakingUp == 1 ? " is" : "s are")
            .Append(" waking up with Vasily Kengele ")
            .Append($"and <b>{notWakingUp}</b> ")
            .Append(notWakingUp == 1 ? "is" : "are")
            .Append(" not.");

        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: messageBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: parameters.CancellationToken);

        if (parameters.User.ChatId == MainChatId)
        {
            foreach (var userRecord in users)
            {
                await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                    text: JsonConvert.SerializeObject(userRecord),
                    cancellationToken: parameters.CancellationToken);
            }
        }
    }
}
