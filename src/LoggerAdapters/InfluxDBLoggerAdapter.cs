namespace VasilyKengele.LoggerAdapters;

public class InfluxDBLoggerAdapter : IUserActionLoggerAdapter
{
    private readonly bool _enabled;
    private readonly string? _token;
    private readonly string? _bucket;
    private readonly string? _organization;

    public InfluxDBLoggerAdapter(IConfiguration configuration)
    {
        if (_enabled = Convert.ToBoolean(configuration["InfluxDB:LoggingToDbEnabled"]))
        {
            _token = configuration["InfluxDB:Token"];
            _bucket = configuration["InfluxDB:Bucket"];
            _organization = configuration["InfluxDB:Organization"];
        }
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
        Console.Write(utcNow.ToLocalTime());
        Console.Write(": ");
        Console.WriteLine(message);

        if (_enabled)
        {
            var point = PointData
                .Measurement("vkLog")
                .Tag("chatId", chatId.ToString())
                .Field("message", message)
                .Timestamp(utcNow, WritePrecision.Ns);

            using var client = InfluxDBClientFactory.Create("http://localhost:8086", _token);
            using var writeApi = client.GetWriteApi();
            writeApi.WritePoint(point, _bucket, _organization);
        }
    }
}
