namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskEmailDispatcher
    {
        /// <summary>
        /// Send mail to user with their status
        /// </summary>
        /// <returns></returns>
         Task DispatchEmailsAsync();
    }
}
