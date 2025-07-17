namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class AuthServiceUnavailableException:Exception
    {
        public AuthServiceUnavailableException()
       : base() { }
        public AuthServiceUnavailableException(string message)
       : base(message) { }
        public AuthServiceUnavailableException(string message, Exception innerException = null)
        : base(message, innerException) { }
    }

    public class TokenRefreshFailedException : Exception
    {
        public TokenRefreshFailedException() { }

        public TokenRefreshFailedException(string message)
            : base(message) { }
        public TokenRefreshFailedException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }



    //public class AuthUnauthorizedException : Exception
    //{
    //    public AuthUnauthorizedException(string message) : base(message) { }
    //}

    //public class AuthBadRequestException : Exception
    //{
    //    public AuthBadRequestException(string message) : base(message) { }
    //}

    //public class AuthInternalException : Exception
    //{
    //    public AuthInternalException(string message) : base(message) { }
    //}


}
