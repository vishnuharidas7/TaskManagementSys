namespace TaskManagementWebAPI.Domain.Exceptions
{
    public class InvalidRoleIdException : Exception
    {
        public InvalidRoleIdException() : base() { }

        public InvalidRoleIdException(string message) : base(message) { }

        public InvalidRoleIdException(string? message, Exception? inner) : base(message, inner) { }
    }
}
