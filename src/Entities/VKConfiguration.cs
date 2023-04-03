namespace VasilyKengele.Entities;

public class VKConfiguration
{
    public required string TelegramBotToken { get; set; }
    public required string MainChatID { get; set; }
    public required InfluxDB InfluxDB { get; set; }
    public required Email Email { get; set; }
    public required OpenAI OpenAI { get; set; }
}

public class InfluxDB
{
    public required bool LoggingToDbEnabled { get; set; }
    public required string Token { get; set; }
    public required string Bucket { get; set; }
    public required string Organization { get; set; }
}

public class Email
{
    public required bool Enabled { get; set; }
    public required string From { get; set; }
    public required Smtp Smtp { get; set; }
}

public class Smtp
{
    public required string Host { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int Port { get; set; }
}

public class OpenAI
{
    public required bool Enabled { get; set; }
    public required string ApiKey { get; set; }
}
