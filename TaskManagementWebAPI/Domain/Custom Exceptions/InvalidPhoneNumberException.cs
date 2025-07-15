using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace TaskManagementWebAPI.Domain.Custom_Exceptions
{
    public class InvalidPhoneNumberException : Exception
    {
        // Parameterless constructor
        public InvalidPhoneNumberException():base()
        {
        }

        // Constructor with a custom message
        public InvalidPhoneNumberException(string? message)
        : base(message)
        {
        }

        // Constructor with a custom message and inner exception
        public InvalidPhoneNumberException(string? message, Exception inner) : base(message, inner)
        {
        }
    }
}
