namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM messages to registered users.
/// </summary>
public class VKBotInvocable : IInvocable
{
    private readonly ITelegramBotClient _botClient;
    private readonly UsersRepositoryService _usersRepository;
    private readonly ILoggerAdapter _logger;
    private readonly InternationalDaysService _internationalDaysService;
    private readonly VKOpenAIService _openAIService;
    private readonly VKEmailService _emailService;

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKBotInvocable(ITelegramBotClient botClient,
                          UsersRepositoryService usersRepository,
                          ILoggerAdapter loggerAdapter,
                          InternationalDaysService internationalDaysService,
                          VKOpenAIService openAIService,
                          VKEmailService emailService)
    {
        _botClient = botClient;
        _usersRepository = usersRepository;
        _logger = loggerAdapter;
        _internationalDaysService = internationalDaysService;
        _openAIService = openAIService;
        _emailService = emailService;
    }

    /// <summary>
    /// This method is invoked at time scheduled by the Coravel library.
    /// <seealso cref="IInvocable.Invoke"/>
    /// </summary>
    public async Task Invoke()
    {
        // Initialize a lazy task that'll call OpenAI API if needed once for all users.
        var quoteTask = new Lazy<Task<string>>(_openAIService.GenerateQuoteAsync);
        var emailInfos = new List<(VKBotUserEntity, string, CancellationToken)>();
        var dayInfo = _internationalDaysService.GetTodaysInternationalDay();

        // Create a wake up message for each user and send to all.
        await Parallel.ForEachAsync(_usersRepository.GetAll(), async (user, token) =>
        {
            if (!user.ReceiveWakeUps)
                return;

            var userTime = DateTime.UtcNow.AddHours(user.UtcDifference);

#if !DEBUG // Release mode: check if it is the correct 5 AM time. Debug mode: skip this check.
            if (userTime.Hour != 5)
                return;
#endif
            // Create message and send it.
            var messageBuilder = new StringBuilder("Hey ")
                .Append(user.Name)
                .Append(", it's <b>")
                .Append(userTime.ToLongDateString())
                .Append("</b> ")
                .Append(userTime.ToLongTimeString())
                .Append('.')
                .AppendLine()
                .Append("Time to wake up!")
                .Append(await quoteTask.Value);

            // Check if today is an international day.
            if (dayInfo is not null)
            {
                messageBuilder.AppendLine().AppendLine()
                    .Append($"Today is ")
                    .Append(dayInfo.Link is not null
                        ? @$"<a href=""{dayInfo.Link}"">{dayInfo.Name}</a>"
                        : dayInfo.Name)
                    .Append('!');
            }

            var messageText = messageBuilder.ToString();
            await SendToTelegramBotAsync(user, messageText, token);

            // Add e-mail info to the list, send it later.
            lock (emailInfos)
            {
                emailInfos.Add((user, messageText, token));
            }
        });
        // Send e-mails to all users sequentially.
        foreach (var (user, text, token) in emailInfos)
        {
            await _emailService.SendToEmailAsync(user, text, token);
        }
    }

    private async Task SendToTelegramBotAsync(VKBotUserEntity user, string messageText, CancellationToken token)
    {
        byte tries = 0;
        do try
        {
            var message = await _botClient.SendTextMessageAsync(user.ChatId, messageText,
                                                                cancellationToken: token,
                                                                parseMode: ParseMode.Html);
            _logger.Log(user.ChatId, "Sent '{0}' to: {1}", message.Text, message.Chat.Id);
            return;
        }
        catch
        {
            tries++;
        }
        while (tries < 3);
        _logger.Log(user.ChatId, "Error sending message to Telegram chat ({0}).", user.ChatId);
    }
}
