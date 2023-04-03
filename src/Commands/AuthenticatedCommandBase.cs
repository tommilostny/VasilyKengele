namespace VasilyKengele.Commands;

/// <summary>
/// Adds main chat ID that a command can use for authentication.
/// Intended for internal commands (loading logs, ...) that can only be accessible by bot the creator.
/// </summary>
public abstract class AuthenticatedCommandBase
{
    protected long MainChatId { get; }

    public AuthenticatedCommandBase(VKConfiguration configuration)
    {
        MainChatId = Convert.ToInt64(configuration.MainChatID);
    }
}
