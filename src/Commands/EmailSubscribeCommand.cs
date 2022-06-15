namespace VasilyKengele.Commands;

public static class EmailSubscribeCommand
{
    public const string Name = "/email";

    public static async Task ExecuteAsync(ITelegramBotClient botClient,
                                           VKBotUsersRepository usersRepository,
                                           VKBotUserEntity user,
                                           string email,
                                           CancellationToken cancellationToken)
    {
        if (_IsValidEmail())
        {
            user.Email = email;
            await usersRepository.UpdateAsync(user);

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
}
