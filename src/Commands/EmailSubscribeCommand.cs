namespace VasilyKengele.Commands;

public class EmailSubscribeCommand : IVKBotCommand
{
    private readonly string _emailArg;

    public EmailSubscribeCommand(string email)
    {
        _emailArg = email;
    }

    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var user = parameters.User;
        
        if (_IsValidEmail())
        {
            user.Email = _emailArg;
            await parameters.UsersRepository.UpdateAsync(user);

            var messageText = user.ReceiveWakeUps
                ? $"Congratulations {user.Name}, you'll now also receive e-mail wake up notifications at {_emailArg}."
                : $"Congratulations {user.Name}, your e-mail address {_emailArg} was stored in our repository.\nActivate with /start to actually receive the notifications.";

            await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                text: messageText,
                cancellationToken: parameters.CancellationToken);
            return;
        }
        await parameters.BotClient.SendTextMessageAsync(user.ChatId,
            text: $"<b>{_emailArg}</b> is not a valid e-mail address. Check the message format example with /help.",
            parseMode: ParseMode.Html,
            cancellationToken: parameters.CancellationToken);

        bool _IsValidEmail() // Inspired by https://stackoverflow.com/a/1374644.
        {
            if (_emailArg is null || _emailArg.EndsWith('.'))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(_emailArg);
                return addr.Address == _emailArg;
            }
            catch
            {
                return false;
            }
        }
    }
}
