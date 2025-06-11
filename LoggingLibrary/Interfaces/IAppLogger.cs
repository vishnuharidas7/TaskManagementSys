namespace LoggingLibrary.Interfaces
{
    public interface IAppLogger<T>
    {
        void LoggInformation(string message,params object[] args);
        void LoggWarning(string message,params object[] args);
        void LoggError(Exception exception, string message,params object[] args); 
    }
}
