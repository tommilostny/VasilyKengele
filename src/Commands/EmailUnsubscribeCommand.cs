namespace VasilyKengele.Commands;

public static class EmailUnsubscribeCommand
{
    public const string Name = "/email_delete";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        if (user.Email is not null)
        {
            user.Email = null;
            await usersRepository.UpdateAsync(user);

            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Your e-mail address has been removed and you'll no longer receive wake up notifications there.",
                cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"You're not receiving wake up notifications via e-mail.",
            cancellationToken: cancellationToken);
    }
}
