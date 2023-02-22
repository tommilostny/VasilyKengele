namespace VasilyKengele.Commands;

internal class KickCommand : AuthenticatedCommandBase, IVKBotCommand
{
    private readonly long _chatId;

    public KickCommand(IConfiguration configuration, long chatID) : base(configuration)
    {
        _chatId = chatID;
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        if (parameters.User.ChatId == MainChatId)
        {
            var user = parameters.UsersRepository.GetById(_chatId);
            if (user is not null)
            {
                await parameters.UsersRepository.RemoveAsync(_chatId);

                await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                    text: $"User '{user.Name}' with ID ({_chatId}) has been removed.",
                    cancellationToken: parameters.CancellationToken);

                await parameters.BotClient.SendTextMessageAsync(_chatId,
                    text: $"You have been removed from the Vasily Kengele system. If you really want to continue, you may restart with /start. Don't forget to correctly set the /time and refer to /help for instructions.",
                    cancellationToken: parameters.CancellationToken);
                return;
            }
            await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
                text: $"Unable to remove user with ID ({_chatId}).",
                cancellationToken: parameters.CancellationToken);
        }
    }
}
