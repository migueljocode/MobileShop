namespace MobileShop.Services.Logging.Interfaces;

public interface IAppLogging<T>
{
    void LogAppError(Exception exception, string message);
    void LogAppError(string message);

    void LogAppCritical(Exception exception, string message);
    void LogAppCritical(string message);

    void LogAppDebug(string message);
    void LogAppTrace(string message);
    void LogAppInformation(string message);
    void LogAppWarning(string message);
}