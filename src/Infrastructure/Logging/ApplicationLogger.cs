using Application.Interfaces;
using Infrastructure.Logging;
using Infrastructure.Logging.Enrichers;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Infrastructure.Logging;

public class ApplicationLogger<T> : IApplicationLogger<T>
{
    private readonly ILogger<T> _logger;
    // Các provider dưới đây là mock, sẽ thay thế bằng implementation thực tế khi phát triển nghiệp vụ
    private readonly ICorrelationIdProvider? _correlationIdProvider;
    private readonly IUserContextProvider? _userContextProvider;

    public ApplicationLogger(ILogger<T> logger, ICorrelationIdProvider? correlationIdProvider = null, IUserContextProvider? userContextProvider = null)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
        _userContextProvider = userContextProvider;
    }

    public void LogConfigurationCreated(Guid configurationId, string configurationType, string userId)
    {
        _logger.LogInformation(LogEvents.ConfigurationCreated + ": {ConfigurationType} {ConfigurationId} {UserId}", configurationType, configurationId, userId);
    }

    public void LogConfigurationUpdated(Guid configurationId, string configurationType, string userId, object changes)
    {
        _logger.LogInformation(LogEvents.ConfigurationUpdated + ": {ConfigurationType} {ConfigurationId} {UserId} {@Changes}", configurationType, configurationId, userId, changes);
    }

    public void LogConfigurationDeleted(Guid configurationId, string configurationType, string userId)
    {
        _logger.LogInformation(LogEvents.ConfigurationDeleted + ": {ConfigurationType} {ConfigurationId} {UserId}", configurationType, configurationId, userId);
    }

    public void LogSecurityEvent(string eventType, string userId, string details, bool success)
    {
        var level = success ? LogLevel.Information : LogLevel.Warning;
        _logger.Log(level, $"Security event {eventType} for user {userId}: {details}. Success: {success}");
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, Dictionary<string, object>? properties = null)
    {
        _logger.LogInformation(LogEvents.OperationTiming + ": {Operation} {Duration}ms {@Properties}", operation, duration.TotalMilliseconds, properties);
    }

    public void LogAuditEvent(string action, string resource, string userId, object details)
    {
        _logger.LogInformation("AUDIT: User {UserId} performed {Action} on {Resource}. Details: {@Details}", userId, action, resource, details);
    }

    public void LogBusinessRuleViolation(string rule, string context, object data)
    {
        _logger.LogWarning("Business rule violation: {Rule} in context {Context}. Data: {@Data}", rule, context, data);
    }
} 