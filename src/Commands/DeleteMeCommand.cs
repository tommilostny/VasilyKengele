namespace VasilyKengele.Commands;

public static class DeleteMeCommand
{
    public const string Name = "/delete_me";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        if (await usersRepository.RemoveAsync(user.ChatId))
        {
            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Goodbye, commrade {user.Name}!\nVasily Kengele is sad to see you leave.",
                cancellationToken: cancellationToken);
        }
    }
}
