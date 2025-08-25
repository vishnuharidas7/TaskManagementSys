namespace TaskManagementWebAPI.Common
{
    /// <summary>
    /// Used for user functionality
    /// </summary>
    public enum UserStatus
    {
        New, 
        PasswordReset
    }

    /// <summary>
    /// Used for task status
    /// </summary>
    public enum TaskStatusInfo
    {
        New,
        Completed,
        Due,
        OverDue 
    }
     
}
