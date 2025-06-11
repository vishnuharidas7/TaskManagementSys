using log4net;
using LoggingLibrary.Interfaces;

namespace LoggingLibrary.Implementations
{
    public class Log4NetLogger<T> : IAppLogger<T>
    {
        private readonly ILog _log;

        public Log4NetLogger()
        {
            _log = LogManager.GetLogger(typeof(T));
        }

        public void LoggInformation(string message, params object[] args)
        {
            if (_log.IsInfoEnabled)
                _log.InfoFormat(message, args);
        }

        public void LoggWarning(string message, params object[] args)
        {
            if (_log.IsWarnEnabled)
                _log.WarnFormat(message, args);
        }

        public void LoggError(Exception exception, string message, params object[] args)
        {
            if (_log.IsErrorEnabled)
                _log.Error(string.Format(message, args), exception);
        }
    }
}
