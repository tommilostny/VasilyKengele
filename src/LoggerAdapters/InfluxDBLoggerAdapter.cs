﻿namespace VasilyKengele.LoggerAdapters;

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

    public async Task<IReadOnlyCollection<string>> ReadLogsAsync(int count, int userUtcDiff)
    {
        if (!_enabled)
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

        var startIndex = records.Count() - count;
        if (startIndex < 0)
        {
            startIndex = 0;
        }
        var logs = new List<string>(count);
        foreach (var record in records.Skip(startIndex))
        {
            var timestamp = record.GetTimeInDateTime();
            if (timestamp is not null)
            {
                logs.Add($"<b>{timestamp.Value.AddHours(userUtcDiff)}</b>: {record.GetValue()}");
                if (logs.Count == count)
                {
                    break;
                }
            }
        }
        return new ReadOnlyCollection<string>(logs);
    }

    private InfluxDBClient CreateDbClient() => InfluxDBClientFactory.Create("http://localhost:8086", _token);
}
