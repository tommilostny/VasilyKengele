namespace VasilyKengele.Commands;

public class AboutMeCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        (var stored, var exists) = await parameters.UsersRepository.GetAsync(user.ChatId, user.Name, user.Username);
        
        if (exists)
        {
            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: JsonConvert.SerializeObject(stored),
                cancellationToken: parameters.CancellationToken);
            return;
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"I do not recognize you, {user.Name}...\nAre you perhaps looking for this? https://www.aboutyou.cz/",
            cancellationToken: parameters.CancellationToken);
    }
}
