namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface TTaskApplicationServices
    {
        /// <summary>
        /// Update task status to due or over due
        /// </summary>
         void UpdateTaskStatuses();
    }
}
