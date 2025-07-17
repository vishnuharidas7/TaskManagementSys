namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class DuplicateUsernameException : Exception
    {
        // Parameterless constructor
        public DuplicateUsernameException() : base()
        {
        }

        // Constructor with a custom message
        public DuplicateUsernameException(string? message)
        : base(message)
        {
        }

        // Constructor with a custom message and inner exception
        public DuplicateUsernameException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
