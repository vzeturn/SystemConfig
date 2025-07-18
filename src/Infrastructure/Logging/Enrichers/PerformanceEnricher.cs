using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Infrastructure.Logging.Enrichers;

public class PerformanceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var memoryUsage = GC.GetTotalMemory(false);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MemoryUsage", memoryUsage));
        using var process = Process.GetCurrentProcess();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessorTime", process.TotalProcessorTime.TotalMilliseconds));
    }
} 