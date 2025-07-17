
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

    //public class UnsupportedFileFormatException : Exception
    //{
    //    public UnsupportedFileFormatException() { }
    //    public UnsupportedFileFormatException(string message) : base(message) { }

    //    public UnsupportedFileFormatException(string message, Exception inner) : base(message, inner) { }
    //}
}