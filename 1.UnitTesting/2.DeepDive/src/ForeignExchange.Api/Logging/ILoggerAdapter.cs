namespace ForeignExchange.Api.Logging;

public interface ILoggerAdapter<T>
{
    void LogInformation(string? message, params object?[] args);
}
