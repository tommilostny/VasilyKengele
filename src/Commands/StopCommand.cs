namespace VasilyKengele.Commands;

public static class StopCommand
{
    public const string Name = "/stop";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        if (user.ReceiveWakeUps)
        {
            user.ReceiveWakeUps = false;
            await usersRepository.UpdateAsync(user);

            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Vasily Kengele will no longer wake you up.\nReactivate with /start.",
                cancellationToken: cancellationToken);
        }
    }
}
