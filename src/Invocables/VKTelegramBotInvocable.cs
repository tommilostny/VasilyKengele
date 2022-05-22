﻿namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM (<seealso cref="Constants.UpdateHour"/>) messages to registered users.
/// </summary>
public class VKTelegramBotInvocable : IInvocable
{
    private readonly TelegramBotClient _botClient;
    private readonly VKBotUsersRepository _usersRepository;

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKTelegramBotInvocable(TelegramBotClient botClient, VKBotUsersRepository usersRepository)
    {
        _botClient = botClient;
        _usersRepository = usersRepository;
    }

    /// <summary>
    /// This method is invoked at time scheduled by the Coravel library.
    /// <seealso cref="IInvocable.Invoke"/>
    /// </summary>
    public async Task Invoke()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var user in _usersRepository.GetAll())
        {
            if (!user.ReceiveWakeUps)
            {
                continue;
            }
            var userTime = utcNow.AddHours(user.UtcDifference);
#if !DEBUG
            if (userTime.Hour == Constants.UpdateHour)
#endif
            {
                if (user.Email is not null)
                {
                    //Send email
                }
                var message = await _botClient.SendTextMessageAsync(user.ChatId, $"Hey {user.Name}, it's {userTime}. Time to wake up!");
                Console.WriteLine($"Sent '{message.Text}' to: {message.Chat.Id}");
            }
        }
    }
}
