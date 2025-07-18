using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging.Filters;

public class SensitiveDataFilter : ILogEventFilter
{
    private static readonly string[] SensitiveProperties =
    {
        "password", "connectionstring", "key", "token", "secret", "credential"
    };
    public bool IsEnabled(LogEvent logEvent)
    {
        return !ContainsSensitiveData(logEvent);
    }
    private bool ContainsSensitiveData(LogEvent logEvent)
    {
        foreach (var property in logEvent.Properties)
        {
            if (SensitiveProperties.Any(sensitive => property.Key.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        return false;
    }
} 