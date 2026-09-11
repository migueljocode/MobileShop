namespace MobileShop.Services.Logging;

public class AppLogging<T>(ILogger<T> logger) : IAppLogging<T>
{
    private readonly ILogger<T> _logger = logger;

    internal static void LogWithException(Exception ex, string message, Action<Exception, string, object[]> logAction)
        => logAction(ex, message, null!);
    

    internal static void LogWithoutException(string message, Action<string, object[]> logAction)
        => logAction(message, null!);

    public void LogAppError(Exception exception, string message)
        => LogWithException(exception, message, _logger.LogError);

    public void LogAppError(string message) 
        => LogWithoutException(message, _logger.LogError);

    public void LogAppCritical(Exception exception, string message) 
        => LogWithException(exception, message, _logger.LogCritical);

    public void LogAppCritical(string message) 
        => LogWithoutException(message, _logger.LogCritical);

    public void LogAppDebug(string message) 
        => LogWithoutException(message, _logger.LogDebug);

    public void LogAppTrace(string message) 
        => LogWithoutException(message, _logger.LogTrace);

    public void LogAppInformation(string message)
        => LogWithoutException(message, _logger.LogInformation);

    public void LogAppWarning(string message) 
        => LogWithoutException(message, _logger.LogWarning);
}