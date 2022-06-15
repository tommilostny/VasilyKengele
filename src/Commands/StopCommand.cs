namespace VasilyKengele.Commands;

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
