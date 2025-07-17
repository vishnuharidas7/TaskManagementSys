namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class InvalidEmailFormatException : Exception
    {
        // Parameterless constructor
        public InvalidEmailFormatException() : base()
        {
        }

        // Constructor with a custom message
        public InvalidEmailFormatException(string? message)
        : base(message)
        {
        }

        // Constructor with a custom message and inner exception
        public InvalidEmailFormatException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
