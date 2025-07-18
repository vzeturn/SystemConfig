using Serilog.Events;

namespace Infrastructure.Logging;

public interface IStructuredLogger
{
    void LogInformation(string messageTemplate, params object[] propertyValues);
    void LogWarning(string messageTemplate, params object[] propertyValues);
    void LogError(Exception exception, string messageTemplate, params object[] propertyValues);
    void LogDebug(string messageTemplate, params object[] propertyValues);
    void LogWithProperties(LogEventLevel level, string messageTemplate, Dictionary<string, object> properties, params object[] propertyValues);
    IDisposable BeginScope(string operationName, Dictionary<string, object>? properties = null);
} 