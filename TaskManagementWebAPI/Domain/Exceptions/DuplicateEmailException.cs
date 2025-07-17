namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        // Parameterless constructor
        public DuplicateEmailException() : base()
        {
        }

        // Constructor with a custom message
        public DuplicateEmailException(string? message)
        : base(message)
        {
        }

        // Constructor with a custom message and inner exception
        public DuplicateEmailException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
