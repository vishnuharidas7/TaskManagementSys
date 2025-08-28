namespace TaskManagementWebAPI.Common
{
    public static class MailMessages
    {
        public const string GreetingTemplate = "Hey {0},\n";
        public const string TaskAssignmentSubject = "New Tasks Assigned to You";
        public const string TaskCompletedSubject = "Task Completed";
        public const string WelcomeMessage = "Welcome to Task Management System – Your Account Details";
        public const string ResetPasswordMsg = "Reset Password – Your Account Details";
        public const string CompletedHeader = "✅ Completed Tasks:";
        public static string TaskLine(string taskType, string taskName, DateTime dueDate)
        {
            return $" - {taskType} {taskName} (Due: {dueDate:MM/dd/yyyy})";
        }
        public const string NewTasksHeader = "🆕 New Tasks Assigned:";

        public static string FormatTaskLine(string redreferenceId, string taskType, string taskName, DateTime dueDate, string priority)
        {
            return $" - Task ID: {redreferenceId} + {taskType} +  {taskName}" + $" (Due: {dueDate}) " + $"(Priotity: {priority})";
        }
        public const string TaskCompletionReminderSubject = "Task Completion Reminder — Action Required";
        public const string ReminderHeader = "⏰ Gentle reminder, Task due dates are approaching.";
        public const string OverdueTaskLineFormat = " - Task ID: {0} {1} {2} (Due: {3:MM/dd/yyyy})";
        public const string ActionReminder = "\nPlease take action on these as soon as possible.";
        public const string TaskDetailsLabel = "\nTask details : \n";
        public const string TaskLineFormat = " - Task ID: {0} {1} {2} (Due: {3:MM/dd/yyyy})";
        public const string ReminderClosing = "\nPlease take action on these as soon as possible.";
        public const string OverdueReminder = "⏰ This is a reminder that the following task exceeded due date and require your attention:";
        public const string WelcomeMessageCreated = "Welcome to Task Management System! Your account has been created successfully\n";
        public const string LoginCredentialsLabel = "Your login credentials : \n";
        public const string Signature = "Regards,\nTask Management System";

        public static class UserOnboarding
        {
            public const string Greeting = "Hey {0},\n";
            public const string UserWelcomeMessage = "Welcome to Task Management System! Your account has been created successfully";
            public const string WelcomeMessageReset = "Your account password has been reset successfully";
            public const string CredentialsIntro = "Your login credentials : \n";
            public const string UsernameLine = "Username: {0}\n";
            public const string EmailLine = "Email: {0}\n";
            public const string PasswordLine = "Password: {0}\n";
            public const string PasswordReminder = "For security reasons, please update your password once you've signed in.\n";
        }
    }
}
