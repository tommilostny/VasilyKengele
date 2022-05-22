namespace VasilyKengele.Handlers;

/// <summary>
/// Class that handles Telegram bot messages.
/// </summary>
public class VKTelegramBotHandler
{
    private readonly VKBotUsersRepository _usersRepository;

    /// <summary>
    /// Initializes <see cref="VKTelegramBotHandler"/> with user entities repository.
    /// </summary>
    /// <param name="usersRepository"><seealso cref="VKBotUsersRepository"/>.</param>
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
        var username = update.Message.Chat.Username;
        var fullname = $"{update.Message.Chat.FirstName} {update.Message.Chat.LastName}";
        (var user, var userExists) = _usersRepository.Get(chatId, fullname, username);
        
        Console.WriteLine($"Received a '{messageText}' message from {fullname} ({username}, {chatId}).");

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
            case Constants.EmailUnsubscribeCommand:
                await ExecuteEmailUnubscribeCommandAsync(botClient, user, cancellationToken);
                break;

            case var timeCommandStr when timeCommandStr.StartsWith(Constants.TimeZoneSetCommand):
                var hourStr = timeCommandStr.Trim().Split(' ').Last();
                if (userExists)
                {
                    await ExecuteTimeZoneUpdateCommandAsync(botClient, user, hourStr, cancellationToken);
                    break;
                }
                await HandleUnknownUserAsync(botClient, user, cancellationToken);
                break;

            case var emailSubscribeStr when emailSubscribeStr.StartsWith(Constants.EmailSubscribeCommand):
                var email = emailSubscribeStr.Trim().Split(' ').Last();
                if (userExists)
                {
                    await ExecuteEmailSubscribeCommandAsync(botClient, user, email, cancellationToken);
                    break;
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
        var helpBuilder = new StringBuilder("<b>Available commands</b>:\n");
        foreach (var command in Constants.GetAllCommands())
        {
            helpBuilder.AppendLine($"{command}: {Constants.HelpStrings[command]}");
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: helpBuilder.ToString(),
            parseMode: ParseMode.Html,
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
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"{hourString} is not a valid hour. Check the message format example with /help.",
            cancellationToken: cancellationToken);
    }

    private async Task ExecuteEmailSubscribeCommandAsync(ITelegramBotClient botClient,
                                                         VKBotUserEntity user,
                                                         string email,
                                                         CancellationToken cancellationToken)
    {
        if (_IsValidEmail())
        {
            user.Email = email;
            await _usersRepository.UpdateAsync(user);

            var messageText = user.ReceiveWakeUps
                ? $"Congratulations {user.Name}, you'll now also receive e-mail wake up notifications at {email}."
                : $"Congratulations {user.Name}, your e-mail address {email} was stored in our repository.\nActivate with /start to actually receive the notifications.";

            await botClient.SendTextMessageAsync(user.ChatId,
                text: messageText,
                cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"<b>{email}</b> is not a valid e-mail address. Check the message format example with /help.",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);

        bool _IsValidEmail() // Inspired by https://stackoverflow.com/a/1374644.
        {
            if (email.EndsWith('.'))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    private async Task ExecuteEmailUnubscribeCommandAsync(ITelegramBotClient botClient,
                                                          VKBotUserEntity user,
                                                          CancellationToken cancellationToken)
    {
        if (user.Email is not null)
        {
            user.Email = null;
            await _usersRepository.UpdateAsync(user);

            await botClient.SendTextMessageAsync(user.ChatId,
                text: $"Your e-mail address has been removed and you'll no longer receive wake up notifications there.",
                cancellationToken: cancellationToken);
            return;
        }
        await botClient.SendTextMessageAsync(user.ChatId,
            text: $"You're not receiving wake up notifications via e-mail.",
            cancellationToken: cancellationToken);
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
