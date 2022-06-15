namespace VasilyKengele.Commands;

public static class UsersCountCommand
{
    public const string Name = "/users_count";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        var users = usersRepository.GetAll();
        var wakingUp = users.Count(u => u.ReceiveWakeUps);
        var notWakingUp = users.Count(u => !u.ReceiveWakeUps);

        var messageBuilder = new StringBuilder($"Right now <b>{wakingUp}</b> users are waking up with Vasily Kengele!")
            .AppendLine()
            .AppendLine($"And <b>{notWakingUp}</b> of the registered users are not waking up with us.");

        await botClient.SendTextMessageAsync(user.ChatId,
            text: messageBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
