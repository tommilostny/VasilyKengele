namespace VasilyKengele.Entities;

public record VKBotUserEntity(long ChatId, string Name)
{
    public int UtcDifference { get; set; }

    public bool TimeZoneSet { get; set; } = false;

    public bool ReceiveWakeUps { get; set; } = false;
}
