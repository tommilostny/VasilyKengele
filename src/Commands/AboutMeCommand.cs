using System.Reflection;

namespace VasilyKengele.Commands;

/// <summary>
/// Loads and presents stored current user info from the repository.
/// </summary>
/// /// <remarks>Command string: <see cref="IVKBotCommand.AboutMe"/></remarks>
public class AboutMeCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        (var stored, var exists) = await parameters.UsersRepository.GetAsync(user.ChatId, user.Name, user.Username);
        
        if (exists)
        {
            var messageBuilder = new StringBuilder("This is all Vasily Kengele knows about you:\n\n");
            var type = stored.GetType();
            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                messageBuilder.AppendLine($"<b>{prop.Name}</b>: {prop.GetValue(stored)}");
            }
            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: messageBuilder.ToString(), //JsonConvert.SerializeObject(stored),
                parseMode: ParseMode.Html,
                cancellationToken: parameters.CancellationToken);
            return;
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"I do not recognize you, {user.Name}...\nAre you perhaps looking for this? https://www.aboutyou.cz/",
            cancellationToken: parameters.CancellationToken);
    }
}
