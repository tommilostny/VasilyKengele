namespace VasilyKengele.Commands;

internal class KickCommand : AuthenticatedCommandBase, IVKBotCommand
{
    private readonly long _chatId;

    public KickCommand(VKConfiguration configuration, long chatID) : base(configuration)
    {
        _chatId = chatID;
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        // Early return if user is not admin.
        if (parameters.User.ChatId != MainChatId)
            return;

        var user = parameters.UsersRepository.GetById(_chatId);
        // Early return if kick target is not registered.
        if (user is null)
        {
            await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                text: $"Unable to remove user with ID ({_chatId}).",
                cancellationToken: parameters.CancellationToken);
            return;
        }
        // Remove user from the repository.
        await parameters.UsersRepository.RemoveAsync(_chatId);

        // Send confirmation message to admin and user.
        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: $"User '{user.Name}' with ID ({_chatId}) has been removed.",
            cancellationToken: parameters.CancellationToken);
        try
        {
            await parameters.BotClient.SendTextMessageAsync(_chatId,
                text: $"You have been removed from the Vasily Kengele system. If you really want to continue, you may restart with /start. Don't forget to correctly set the /time and refer to /help for instructions.",
                cancellationToken: parameters.CancellationToken);
        }
        catch
        {
            await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                text: $"Unable to send message to {_chatId}. This user probably does not exist anymore.",
                cancellationToken: parameters.CancellationToken);
        }
    }
}
