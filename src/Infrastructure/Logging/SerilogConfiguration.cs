using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static ILogger CreateLogger(IConfiguration configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithCorrelationId()
            .WriteTo.File(
                path: Path.Combine("logs", "application-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext:l} {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.EventLog(
                source: "SystemConfig",
                logName: "Application",
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.Conditional(
                condition: evt => evt.Level >= LogEventLevel.Error,
                configureSink: sink => sink.File(
                    path: Path.Combine("logs", "errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90))
            .Filter.ByExcluding(Matching.FromSource("Microsoft"))
            .Filter.ByExcluding(Matching.FromSource("System"));
        return loggerConfig.CreateLogger();
    }
} 