﻿namespace VasilyKengele.Commands;

public class UsersCountCommand : IVKBotCommand
{
    public async Task ExecuteAsync(CommandParameters parameters)
    {
        var users = parameters.UsersRepository.GetAll();
        var wakingUp = users.Count(u => u.ReceiveWakeUps);
        var notWakingUp = users.Count(u => !u.ReceiveWakeUps);

        var messageBuilder = new StringBuilder($"Right now <b>{wakingUp}</b> user")
            .Append(wakingUp == 1 ? " is" : "s are")
            .Append(" waking up with Vasily Kengele ")
            .Append($"and <b>{notWakingUp}</b> ")
            .Append(notWakingUp == 1 ? "is" : "are")
            .Append(" not.");

        await parameters.BotClient.SendTextMessageAsync(parameters.User.ChatId,
            text: messageBuilder.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: parameters.CancellationToken);
    }
}