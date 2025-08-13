using Microsoft.AspNetCore.Http.HttpResults;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Common.ExceptionMessages
{
    public static class ExceptionMessages
    {
        public static class TaskExceptions
        {
            public const string ReferenceIdConflict = "Failed to upload tasks due to repeated referenceId conflicts.";
            public const string TaskNotFoundByUserId = "Task with UserID {0} not found.";
        }

        public static class UserAuthExceptions
        {
            public const string TokenRefreshFailed = "Token refresh failed: {0}";
            public const string TokenRefreshServiceUnavailable = "Token refresh service is unavailable.";
            public const string TokenRefreshTimeout = "Token refresh timed out.";
            public const string UserNotFound = "User not found";
        }
        public static class ExceptionHandelingExceptions
        {
            public const string UnauthorizedAccess = "Unauthorized access.";
            public const string NullValuesNotAllowed = "Null values are not acceptable.";
            public const string ExternalServiceUnavailable = "External service unavailable.";
            public const string RequestTimedOut = "Request timed out.";
            public const string InvalidOperation = "Invalid operation.";
            public const string UnexpectedError = "An unexpected error occurred.";
        }

    }
}
