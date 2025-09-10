
namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class TaskFileParserException : Exception
    {
        public TaskFileParserException() { }
        public TaskFileParserException(string message) : base(message) { }
        public TaskFileParserException(string message,Exception innerException) : base(message, innerException) { }
    }

    public class TaskValidationException : Exception
    {
        public TaskValidationException() { }
        public TaskValidationException(string message) : base(message) { }
        public TaskValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}