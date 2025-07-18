namespace Infrastructure.Logging;

public static class LogEvents
{
    // Application Events
    public const string ConfigurationCreated = "Configuration.Created";
    public const string ConfigurationUpdated = "Configuration.Updated";
    public const string ConfigurationDeleted = "Configuration.Deleted";
    public const string BackupCreated = "Backup.Created";
    public const string BackupRestored = "Backup.Restored";
    // Security Events
    public const string UserAuthenticated = "Security.UserAuthenticated";
    public const string UnauthorizedAccess = "Security.UnauthorizedAccess";
    public const string PermissionDenied = "Security.PermissionDenied";
    public const string DataEncrypted = "Security.DataEncrypted";
    public const string DataDecrypted = "Security.DataDecrypted";
    // Performance Events
    public const string OperationTiming = "Performance.OperationTiming";
    public const string CacheHit = "Performance.CacheHit";
    public const string CacheMiss = "Performance.CacheMiss";
    public const string DatabaseQuery = "Performance.DatabaseQuery";
    // Error Events
    public const string Exception = "Error.Exception";
    public const string ValidationFailed = "Error.ValidationFailed";
    public const string ConfigurationError = "Error.ConfigurationError";
    public const string SystemError = "Error.SystemError";
}

public static class LogProperties
{
    public const string UserId = "UserId";
    public const string OperationId = "OperationId";
    public const string ConfigurationId = "ConfigurationId";
    public const string Duration = "Duration";
    public const string ErrorCode = "ErrorCode";
    public const string IPAddress = "IPAddress";
    public const string UserAgent = "UserAgent";
    public const string RequestPath = "RequestPath";
} 