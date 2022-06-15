namespace VasilyKengele.Commands;

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
