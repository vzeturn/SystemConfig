namespace Application.Interfaces;

public interface IApplicationLogger<T>
{
    void LogConfigurationCreated(Guid configurationId, string configurationType, string userId);
    void LogConfigurationUpdated(Guid configurationId, string configurationType, string userId, object changes);
    void LogConfigurationDeleted(Guid configurationId, string configurationType, string userId);
    void LogSecurityEvent(string eventType, string userId, string details, bool success);
    void LogPerformanceMetric(string operation, TimeSpan duration, Dictionary<string, object>? properties = null);
    void LogBusinessRuleViolation(string rule, string context, object data);
    void LogAuditEvent(string action, string resource, string userId, object details);
} 