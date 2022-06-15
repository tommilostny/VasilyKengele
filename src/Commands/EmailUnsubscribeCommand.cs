namespace VasilyKengele.Commands;

public class EmailUnsubscribeCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        if (user.Email is not null)
        {
            user.Email = null;
            await parameters.UsersRepository.UpdateAsync(user);

            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: $"Your e-mail address has been removed and you'll no longer receive wake up notifications there.",
                cancellationToken: parameters.CancellationToken);
            return;
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"You're not receiving wake up notifications via e-mail.",
            cancellationToken: parameters.CancellationToken);
    }
}
