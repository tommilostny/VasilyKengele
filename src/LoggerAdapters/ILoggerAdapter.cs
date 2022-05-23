namespace VasilyKengele.LoggerAdapters;

public interface IUserActionLoggerAdapter
{
    void Log(long chatId, string message);

    void Log<T0>(long chatId, string message, T0 arg0);

    void Log<T0, T1>(long chatId, string message, T0 arg0, T1 arg1);

    void Log<T0, T1, T2>(long chatId, string message, T0 arg0, T1 arg1, T2 arg2);

    void Log(long chatId, string message, params object?[] args);
}
