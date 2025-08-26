namespace TaskManagementWebAPI.Common
{
    public static class UserAPIEndpoints
    {
        public const string Base = "api/Users";

        public static class Get
        {
            public const string CheckUsername = "check-username";
            public const string UserList = "viewusers";
            public const string UserListById = "viewusersByid/{id}";
        }

        public static class Post
        {
            public const string Register = "register";
        }

        public static class Put
        {
            public const string UpdateUser = "updateuser/{id}";
            public const string UpdatePassword = "updatePswd/{id}";
        }

        public static class Delete
        {
            public const string DeleteUser = "deleteUser/{id}";
        }

    }
}
