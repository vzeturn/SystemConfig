using Serilog.Events;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

public class StructuredLogger : IStructuredLogger
{
    private readonly ILogger<StructuredLogger> _logger;

    public StructuredLogger(ILogger<StructuredLogger> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogInformation(messageTemplate, propertyValues);
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogWarning(messageTemplate, propertyValues);
    }

    public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        _logger.LogError(exception, messageTemplate, propertyValues);
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogDebug(messageTemplate, propertyValues);
    }

    public void LogWithProperties(LogEventLevel level, string messageTemplate, Dictionary<string, object> properties, params object[] propertyValues)
    {
        using var scope = _logger.BeginScope(properties);
        _logger.Log((Microsoft.Extensions.Logging.LogLevel)level, messageTemplate, propertyValues);
    }

    public IDisposable BeginScope(string operationName, Dictionary<string, object>? properties = null)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["OperationId"] = Guid.NewGuid().ToString()
        };
        if (properties != null)
        {
            foreach (var prop in properties)
                scopeProperties[prop.Key] = prop.Value;
        }
        return _logger.BeginScope(scopeProperties);
    }
} 