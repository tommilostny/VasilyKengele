namespace VasilyKengele.LoggerAdapters;

/// <summary>
/// Implementation of the logger adapter that stores logs in the InfluxDB time series oriented database.
/// </summary>
public class InfluxDBLoggerAdapter : ILoggerAdapter
{
    private readonly ILogger<InfluxDBLoggerAdapter> _logger;
    private readonly bool _enabled;
    private readonly string? _token;
    private readonly string? _bucket;
    private readonly string? _organization;

    public InfluxDBLoggerAdapter(IConfiguration configuration, ILogger<InfluxDBLoggerAdapter> logger)
    {
        if (_enabled = Convert.ToBoolean(configuration["InfluxDB:LoggingToDbEnabled"]))
        {
            _token = configuration["InfluxDB:Token"];
            _bucket = configuration["InfluxDB:Bucket"];
            _organization = configuration["InfluxDB:Organization"];
        }
        _logger = logger;
    }

    public void Log(long chatId, string message)
    {
        WriteMessage(chatId, message);
    }

    public void Log<T0>(long chatId, string message, T0 arg0)
    {
        var fullMessage = string.Format(message, arg0);
        WriteMessage(chatId, fullMessage);
    }

    public void Log<T0, T1>(long chatId, string message, T0 arg0, T1 arg1)
    {
        var fullMessage = string.Format(message, arg0, arg1);
        WriteMessage(chatId, fullMessage);
    }

    public void Log<T0, T1, T2>(long chatId, string message, T0 arg0, T1 arg1, T2 arg2)
    {
        var fullMessage = string.Format(message, arg0, arg1, arg2);
        WriteMessage(chatId, fullMessage);
    }

    public void Log(long chatId, string message, params object?[] args)
    {
        var fullMessage = string.Format(message, args);
        WriteMessage(chatId, fullMessage);
    }

    private void WriteMessage(long chatId, string message)
    {
        var utcNow = DateTime.UtcNow;
        _logger.LogInformation("{timestamp}: {message}", utcNow.ToLocalTime(), message);

        if (_enabled)
        {
            var point = PointData
                .Measurement("vkLog")
                .Tag("chatId", chatId.ToString())
                .Field("message", message)
                .Timestamp(utcNow, WritePrecision.Ns);

            using var client = CreateDbClient();
            using var writeApi = client.GetWriteApi();
            writeApi.WritePoint(point, _bucket, _organization);
        }
    }

    private InfluxDBClient CreateDbClient() => InfluxDBClientFactory.Create("http://localhost:8086", _token);

    public async Task ReadLogsToBotAsync(int count, CommandParameters parameters)
    {
        if (!_enabled || count < 0)
        {
            throw new InvalidOperationException();
        }
        var queryBuilder = new StringBuilder()
            .Append($"from(bucket: \"{_bucket}\")")
            .Append(" |> range(start: -30d)")
            .Append(" |> filter(fn: (r) => r[\"_measurement\"] == \"vkLog\")")
            .Append(" |> filter(fn: (r) => r[\"_field\"] == \"message\")")
            .Append(" |> filter(fn: (r) => r[\"chatId\"] != \"-1\")");

        using var client = CreateDbClient();
        var tables = await client.GetQueryApi().QueryAsync(queryBuilder.ToString(), _organization);
        var records = tables.SelectMany(table => table.Records);
        
        var user = parameters.User;
        var startIndex = Math.Max(records.Count() - count, 0);

        foreach (var record in records.Skip(startIndex))
        {
            var timestamp = record.GetTimeInDateTime();
            if (timestamp is not null)
            {
                await parameters.BotClient.SendTextMessageAsync(user.ChatId,
                    text: $"<b>{timestamp.Value.AddHours(user.UtcDifference)}</b>: {record.GetValue()}",
                    parseMode: ParseMode.Html);

                if (--count == 0)
                    break;
            }
        }
    }
}
