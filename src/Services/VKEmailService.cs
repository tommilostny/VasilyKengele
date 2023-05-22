namespace VasilyKengele.Services;

public class VKEmailService
{
    private readonly IFluentEmailFactory? _fluentEmailFactory;
    private readonly ILoggerAdapter _logger;

    public VKEmailService(VKConfiguration configuration,
                          IFluentEmailFactory fluentEmailFactory,
                          ILoggerAdapter logger)
    {
        if (configuration.Email.Enabled)
        {
            _fluentEmailFactory = fluentEmailFactory;
        }
        _logger = logger;
    }

    public async Task SendToEmailAsync(VKBotUserEntity user, string messageText, CancellationToken token)
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
