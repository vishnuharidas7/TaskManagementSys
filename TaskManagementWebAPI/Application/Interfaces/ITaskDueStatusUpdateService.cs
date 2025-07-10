namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface ITaskDueStatusUpdateService
    {
        /// <summary>
        /// Update task status to due or over due
        /// </summary>
         void UpdateTaskStatuses();
    }
}
