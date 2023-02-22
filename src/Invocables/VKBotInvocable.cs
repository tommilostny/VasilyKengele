namespace VasilyKengele.Invocables;

/// <summary>
/// Class that is used to send daily 5 AM messages to registered users.
/// </summary>
public class VKBotInvocable : IInvocable
{
    private readonly ITelegramBotClient _botClient;
    private readonly VKBotUsersRepository _usersRepository;
    private readonly ILoggerAdapter _logger;
    private readonly IFluentEmailFactory? _fluentEmailFactory;
    private readonly IOpenAIService? _openAIService;

    private readonly Random _random = new();
    private readonly string[] _adjectives = new[]
    {
        "Insightful", "Enlightening", "Capitalist", "Socialist",
        "Intriguing", "Uplifting", "Provocative", "Crazy",
        "Thought-provoking", "Stimulating", "Remarkable",
        "Funny", "Sad", "Anarchist", "Rebelious", "Stupid",
        "Compelling", "Stimulating", "Profound", "Informal",
        "Affecting", "Moving", "Encouraging", "Pertinent",
        "Irrelevant", "Diverting", "Innovative", "Idiotic",
        "Gratifying", "Edifying", "Technical", "Programming"
    };

    /// <summary>
    /// Loads the Telegram bot token and stores a reference to the users repository.
    /// </summary>
    public VKBotInvocable(ITelegramBotClient botClient,
                          VKBotUsersRepository usersRepository,
                          ILoggerAdapter loggerAdapter,
                          IFluentEmailFactory fluentEmailFactory,
                          IConfiguration configuration,
                          IOpenAIService openAIService)
    {
        if (!string.IsNullOrWhiteSpace(configuration["Email:From"]))
        {
            _fluentEmailFactory = fluentEmailFactory;
        }
        if (Convert.ToBoolean(configuration["OpenAI:Enabled"]))
        {
            _openAIService = openAIService;
        }
        _botClient = botClient;
        _usersRepository = usersRepository;
        _logger = loggerAdapter;
    }

    /// <summary>
    /// This method is invoked at time scheduled by the Coravel library.
    /// <seealso cref="IInvocable.Invoke"/>
    /// </summary>
    public async Task Invoke()
    {
        // Initialize a lazy task that'll call OpenAI API if needed once for all users.
        var quoteTask = new Lazy<Task<string>>(GenerateQuoteAsync); 

        // Create a wake up message for each user and send to all.
        await Parallel.ForEachAsync(_usersRepository.GetAll(), async (user, token) =>
        {
            if (!user.ReceiveWakeUps)
                return;

            var userTime = DateTime.UtcNow.AddHours(user.UtcDifference);

#if !DEBUG //Release mode: check if it is the correct 5 AM time. Debug mode: skip this check.
            if (userTime.Hour != 5)
                return;
#endif
            //Create message and send it.
            var messageBuilder = new StringBuilder("Hey ")
                .Append(user.Name)
                .Append(", it's ")
                .Append(userTime)
                .Append(". Time to wake up!")
                .Append(await quoteTask.Value);

            var messageText = messageBuilder.ToString();
            await SendToTelegramBotAsync(user, messageText, token);
            await SendToEmailAsync(user, messageText, token);
        });
    }

    /// <summary>
    /// Task that calls Open AI API to generate a bird quote.
    /// The API is not going to be called if no message is about to be sent
    /// (i.e. there is no user in a time zone where it is currently 5 AM).
    /// </summary>
    private async Task<string> GenerateQuoteAsync()
    {
        if (_openAIService is not null)
        {
            var nextAdjective = _adjectives[_random.Next(0, _adjectives.Length)];
        
            var completion = await _openAIService.Completions.CreateCompletion(new()
            {
                Prompt = $"{nextAdjective} bird quote for the day {DateTime.UtcNow}:",
                Model = Models.TextDavinciV3,
                Temperature = 1.0f,
                MaxTokens = 300
            });
            if (completion.Successful)
            {
                return completion.Choices.First().Text;
            }
        }
        return string.Empty;
    }

    private async Task SendToTelegramBotAsync(VKBotUserEntity user, string messageText, CancellationToken token)
    {
        byte tries = 0;
        do try
        {
            var message = await _botClient.SendTextMessageAsync(user.ChatId, messageText, cancellationToken: token);
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

    private async Task SendToEmailAsync(VKBotUserEntity user, string messageText, CancellationToken token)
    {
        //Send message to e-mail if user has it setup.
        //Or skip if e-mail sending is not enabled in configuration.
        if (user.Email is null || _fluentEmailFactory is null)
            return;

        var email = _fluentEmailFactory.Create()
            .To(user.Email)
            .Subject("Wake up with Vasily Kengele")
            .Body(messageText);

        var emailResult = await email.SendAsync(token);
        if (emailResult.Successful)
        {
            _logger.Log(user.ChatId, "Sent e-mail '{0}' to {1}", messageText, user.Email);
            return;
        }
        _logger.Log(user.ChatId, "Unable to send e-mail to {0}", user.Email);
    }
}
