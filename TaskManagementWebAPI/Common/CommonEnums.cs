namespace TaskManagementWebAPI.Common
{
    /// <summary>
    /// Used for user functionality
    /// </summary>
    public enum UserEnums
    {
        New, 
        PasswordReset
    }

    /// <summary>
    /// Used for task status
    /// </summary>
    public enum TaskStatusEnums
    {
        New,
        Completed,
        Due,
        OverDue 
    }
     
}
