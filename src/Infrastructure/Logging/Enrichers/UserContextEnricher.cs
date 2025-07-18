using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging.Enrichers;

public interface IUserContextProvider
{
    UserContext? GetCurrentUser();
}

public interface ICorrelationIdProvider { string GetCorrelationId(); }

public class UserContext
{
    public string UserId { get; set; } = "anonymous";
    public string UserName { get; set; } = "anonymous";
    public string Role { get; set; } = "User";
}

public class UserContextEnricher : ILogEventEnricher
{
    private readonly IUserContextProvider _userContextProvider;
    public UserContextEnricher(IUserContextProvider userContextProvider)
    {
        _userContextProvider = userContextProvider;
    }
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userContext = _userContextProvider.GetCurrentUser();
        if (userContext != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userContext.UserId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userContext.UserName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRole", userContext.Role));
        }
    }
} 