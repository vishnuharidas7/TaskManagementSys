namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class AuthServiceUnavailableException:Exception
    {
        public AuthServiceUnavailableException()
       : base() { }
        public AuthServiceUnavailableException(string message)
       : base(message) { }
        public AuthServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException) { }
    }

    public class TokenRefreshFailedException : Exception
    {
        public TokenRefreshFailedException() { }

        public TokenRefreshFailedException(string message)
            : base(message) { }
        public TokenRefreshFailedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
