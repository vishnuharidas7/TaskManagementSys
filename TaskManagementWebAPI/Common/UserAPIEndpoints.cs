namespace TaskManagementWebAPI.Common
{
    public static class UserAPIEndpoints
    {
        public const string Base = "api/Users";

        public static class Get
        {
            public const string CheckUsername = Base + "/check-username";
            public const string UserList = Base + "/viewusers";
            public const string UserListById = Base + "/viewusersByid/{id}";
        }

        public static class Post
        {
            public const string Register = Base + "/register";
        }

        public static class Put
        {
            public const string UpdateUser = Base + "/updateuser/{id}";
            public const string UpdatePassword = Base + "/updatePswd/{id}";
        }

        public static class Delete
        {
            public const string DeleteUser = Base + "/deleteUser/{id}";
        }

    }
}
