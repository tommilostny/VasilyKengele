namespace VasilyKengele.Commands;

public static class AboutMeCommand
{
    public const string Name = "/about_me";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                          VKBotUsersRepository usersRepository,
                                          VKBotUserEntity user,
                                          CancellationToken cancellationToken)
    {
        (var stored, var exists) = await usersRepository.GetAsync(user.ChatId, user.Name, user.Username);
        if (exists)
        {
            await botClient.SendTextMessageAsync(user.ChatId,
                text: JsonConvert.SerializeObject(stored),
                cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"I do not recognize you, {user.Name}...\nAre you perhaps looking for this? https://www.aboutyou.cz/",
            cancellationToken: cancellationToken);
    }
}
