namespace TaskManagementWebAPI.Common.ExceptionMessages
{
    public static class ExceptionMessages
    {
        public static class UserExceptions
        {
            public const string UsernameRequired = "Username cannot be null or empty.";
            public const string UserNotFound = "User not found.";
            public const string UsernameAlreadyExists = "Username already exists.";
            public const string UserCannotBeDeleted = "Cannot delete user. Tasks are assigned to this user.";
            public const string PasswordNotMatch = "New password and confirmation do not match.";
            public const string InvalidCrendentials = "Current password is incorrect";
        }

        public static class TaskExceptions
        {
            public const string FailedTaskEntryRefIdConflict = "Failed to add task after multiple attempts due to reference ID conflicts.";
            public const string ParseNotFound = "No parser found for file.";
            public const string ParseEmpty = "Parsed data cannot be empty or null.";
            public const string CannotMapped = "No tasks could be mapped from the file.";
            public const string InvalidDateFormat = "Unable to parse the task file due to invalid format.";
            public const string TaskNotFound = "Task not found.";
        }

        public static class  Validations
        {
            public const string InvalidPhoneNumber = "Invalid phone number";
            public const string InvalidEmailFormat = "Invalid Email Format ";
            public const string DuplicateEmail = "Email already exists ";
            public const string InvalidRole = "Invalid RoleId ";
            public const string DuplicateUsername = "Username already exists";
        }
    }
}
