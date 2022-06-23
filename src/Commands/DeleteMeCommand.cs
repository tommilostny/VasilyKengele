namespace VasilyKengele.Commands;

/// <summary>
/// Deletes the calling user data from the repository if they changed their mind and don't want to use the bot anymore and leave the data with us.
/// </summary>
/// /// <remarks>Command string: <see cref="IVKBotCommand.DeleteMe"/></remarks>
public class DeleteMeCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        if (await parameters.UsersRepository.RemoveAsync(parameters.User.ChatId))
        {
            await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                text: $"Goodbye, commrade {parameters.User.Name}!\nVasily Kengele is sad to see you leave.",
                cancellationToken: parameters.CancellationToken);
        }
    }
}
