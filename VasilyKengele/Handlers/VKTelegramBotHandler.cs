namespace VasilyKengele.Handlers;

/// <summary>
/// Class that handles Telegram bot messages.
/// </summary>
public class VKTelegramBotHandler
{
    private readonly VKBotUsersRepository _usersRepository;

    public VKTelegramBotHandler(VKBotUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient,
                                        Update update,
                                        CancellationToken cancellationToken)
    {
        if (update.Message!.Type != MessageType.Text) // Only process text messages
        {
            return;
        }
        var messageText = update.Message.Text;
        if (messageText is null)
        {
            return;
        }
        var chatId = update.Message.Chat.Id;
        var username = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";
        (var user, var userExists) = _usersRepository.Get(chatId, username);
        
        Console.WriteLine($"Received a '{messageText}' message from {username} ({chatId}).");

        switch (messageText)
        {
            case Constants.StartCommand:
                await ExecuteStartCommandAsync(botClient, user, cancellationToken);
                break;
            case Constants.StopCommand:
                await ExecuteStopCommandAsync(botClient, user, cancellationToken);
                break;
            case Constants.UsersCountCommand:
                await ExecuteUsersCountCommandAsync(botClient, user, cancellationToken);
                break;
            case Constants.AboutMeCommand:
                await ExecuteAboutMeCommandAsync(botClient, user, cancellationToken);
                break;
            case Constants.DeleteMeCommand:
                await ExecuteDeleteMeCommandAsync(botClient, user, cancellationToken);
                break;
            case Constants.HelpCommand:
                await ExecuteHelpCommand(botClient, user, cancellationToken);
                break;
            case var timeCommandStr when timeCommandStr.StartsWith(Constants.TimeZoneSetCommand):
                var hourStr = timeCommandStr.Split(' ').Last();
                if (userExists)
                {
                    await ExecuteTimeZoneUpdateCommandAsync(botClient, user, hourStr, cancellationToken);
                }
                await HandleUnknownUserAsync(botClient, user, cancellationToken);
                break;
            default:
                await HandleUnknownCommandAsync(botClient, user, cancellationToken);
                break;
        }
    }

    private async Task ExecuteStartCommandAsync(ITelegramBotClient botClient,
                                                VKBotUserEntity user,
                                                CancellationToken cancellationToken)
    {
        if (!user.ReceiveWakeUps)
        {
            await _usersRepository.AddAsync(user);
            var messageBuilder = new StringBuilder()
                .AppendLine($"Vasily Kengele welcomes you, commrade {user.Name}!");

            if (!user.TimeZoneSet)
            {
                messageBuilder.AppendLine($"Wake up with us at {Constants.UpdateHour} o'clock.")
                    .Append($"To do that Vasily needs to know your timezone by using {Constants.TimeZoneSetCommand} command.")
                    .AppendLine($" Use the {Constants.HelpCommand} command for more info.");
            }
            else
            {
                messageBuilder.AppendLine($"You will now receive {Constants.UpdateHour} o'clock wake ups from Vasily again.");
                user.ReceiveWakeUps = true;
                await _usersRepository.UpdateAsync(user);
            }

            await botClient.SendTextMessageAsync(user.ChatId,
                    text: messageBuilder.ToString(),
                    cancellationToken: cancellationToken);
        }
    }

    private async Task ExecuteStopCommandAsync(ITelegramBotClient botClient,
                                               VKBotUserEntity user,
                                               CancellationToken cancellationToken)
    {
        if (user.ReceiveWakeUps)
        {
            user.ReceiveWakeUps = false;
            await _usersRepository.UpdateAsync(user);

            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Vasily Kengele will no longer wake you up.\nReactivate with /start.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task ExecuteUsersCountCommandAsync(ITelegramBotClient botClient,
                                                     VKBotUserEntity user,
                                                     CancellationToken cancellationToken)
    {
        var count = _usersRepository.GetAll().Count(u => u.ReceiveWakeUps);

        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"Right now {count} users are waking up with Vasily Kengele!",
            cancellationToken: cancellationToken);
    }

    private static async Task ExecuteHelpCommand(ITelegramBotClient botClient,
                                                 VKBotUserEntity user,
                                                 CancellationToken cancellationToken)
    {
        var helpBuilder = new StringBuilder("Available commands:\n");
        foreach (var command in Constants.GetAllCommands())
        {
            helpBuilder.AppendLine($"{command}: {Constants.HelpStrings[command]}");
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: helpBuilder.ToString(),
            cancellationToken: cancellationToken);
    }

    private async Task ExecuteTimeZoneUpdateCommandAsync(ITelegramBotClient botClient,
                                                         VKBotUserEntity user,
                                                         string hourString,
                                                         CancellationToken cancellationToken)
    {
        if (hourString.All(c => Char.IsDigit(c)))
        {
            var currentHour = Convert.ToInt32(hourString);
            if (currentHour >= 0 && currentHour < 24)
            {
                user.TimeZoneSet = user.ReceiveWakeUps = true;
                user.UtcDifference = currentHour - DateTime.UtcNow.Hour;
                await _usersRepository.UpdateAsync(user);

                var messageBuilder = new StringBuilder($"Congratulations {user.Name}, your timezone was set to UTC");
                if (user.UtcDifference >= 0)
                {
                    messageBuilder.Append('+');
                }
                messageBuilder.AppendLine($"{user.UtcDifference}!")
                    .AppendLine($"You will now receive {Constants.UpdateHour} o'clock wake ups from Vasily Kengele in correct time.");

                await botClient.SendTextMessageAsync(user.ChatId,
                    text: messageBuilder.ToString(),
                    cancellationToken: cancellationToken);
                return;
            }
        }
        await HandleUnknownCommandAsync(botClient, user, cancellationToken);
    }

    private async Task ExecuteAboutMeCommandAsync(ITelegramBotClient botClient,
                                                  VKBotUserEntity user,
                                                  CancellationToken cancellationToken)
    {
        (var stored, var exists) = _usersRepository.Get(user.ChatId, user.Name);
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

    private async Task ExecuteDeleteMeCommandAsync(ITelegramBotClient botClient,
                                               VKBotUserEntity user,
                                               CancellationToken cancellationToken)
    {
        if (await _usersRepository.RemoveAsync(user))
        {
            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Goodbye, commrade {user.Name}!\nVasily Kengele is sad to see you leave.",
                cancellationToken: cancellationToken);
        }
    }

    private static async Task HandleUnknownCommandAsync(ITelegramBotClient botClient,
                                                        VKBotUserEntity user,
                                                        CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"Vasily Kengele did not understand you!\nLearn available commands from /help.",
            cancellationToken: cancellationToken);
    }

    private static async Task HandleUnknownUserAsync(ITelegramBotClient botClient,
                                                     VKBotUserEntity user,
                                                     CancellationToken cancellationToken)
    {
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"Vasily Kengele does not recognize you.\nReactivate with /start.",
            cancellationToken: cancellationToken);
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient,
                                        Exception exception,
                                        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.Error.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
