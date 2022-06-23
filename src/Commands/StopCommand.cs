namespace VasilyKengele.Commands;

/// <summary>
/// Disables the bot notifications at 5 AM. User data remain stored in the repository and are still available for future use.
/// </summary>
/// <remarks>Command string: <see cref="IVKBotCommand.Stop"/></remarks>
public class StopCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        if (user.ReceiveWakeUps)
        {
            user.ReceiveWakeUps = false;
            await parameters.UsersRepository.UpdateAsync(user);

            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: "Vasily Kengele will no longer wake you up.\nReactivate with /start.",
                cancellationToken: parameters.CancellationToken);
        }
    }
}
