namespace VasilyKengele.Entities;

/// <summary>
/// Record class that represents stored user data.
/// </summary>
/// <param name="ChatId">Telegram user chat ID.</param>
/// <param name="Name">Telegram user first and last name.</param>
public record VKBotUserEntity(long ChatId, string Name)
{
    public int UtcDifference { get; set; }

    public bool TimeZoneSet { get; set; } = false;

    public bool ReceiveWakeUps { get; set; } = false;

    public string? Email { get; set; }
}
