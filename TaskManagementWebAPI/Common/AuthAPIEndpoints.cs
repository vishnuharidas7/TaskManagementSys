namespace TaskManagementWebAPI.Common
{
    public static class AuthAPIEndpoints
    {
        public const string Base = "api/Auth";
         

        public static class Post
        {
            public const string Login = "/LoginAuth";
            public const string Refresh =  "/refresh";
            public const string ForgotPassword = "/forgot-password";
        }

        
    }
}
