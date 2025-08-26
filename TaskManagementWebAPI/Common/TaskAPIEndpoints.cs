namespace TaskManagementWebAPI.Common
{
    public static class TaskAPIEndpoints
    {
        public const string Base = "api/Tasks";

        public static class Get
        {
            public const string AssignUser = "/AssignUser";
            public const string ViewAllTasks = "/ViewAllTasks";
            public const string GetTasksByUserId = "/task/{userId:int}";
            public const string GetTaskNotificationsByUserId = "/taskNotification/{userId:int}";
            public const string GetTaskNotificationsByAdmin = "/taskNotificationAdmin";
        }

        public static class Post
        {
            public const string AddTask = "/AddTask";
            public const string FileUpload = "/upload";
            public const string UpdateTaskStatuses = "/update-Taskstatuses";
            public const string SendOverdueTaskMail = "/send-overduetaskmail";
        }

        public static class Put
        {
            public const string UpdateTask = "/UpdateTask/{id}";  
        }

        public static class Delete
        {
            public const string DeleteTask = "/deleteTask/{id}";
        }
    }
}
