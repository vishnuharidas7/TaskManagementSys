namespace TaskManagementWebAPI.Common
{
    public static class TaskAPIEndpoints
    {
        public const string Base = "api/Tasks";

        public static class Get
        {
            public const string AssignUser = Base + "/AssignUser";
            public const string ViewAllTasks = Base + "/ViewAllTasks";
            public const string GetTasksByUserId = Base + "/task/{userId:int}";
            public const string GetTaskNotificationsByUserId = Base + "/taskNotification/{userId:int}";
            public const string GetTaskNotificationsByAdmin = Base + "/taskNotificationAdmin";
        }

        public static class Post
        {
            public const string AddTask = Base + "/AddTask";
            public const string FileUpload = Base + "/upload";
            public const string UpdateTaskStatuses = Base + "/update-Taskstatuses";
            public const string SendOverdueTaskMail = Base + "/send-overduetaskmail";
        }

        public static class Put
        {
            public const string UpdateTask = Base + "/UpdateTask/{id}";  
        }

        public static class Delete
        {
            public const string DeleteTask = Base + "/deleteTask/{id}";
        }
    }
}
