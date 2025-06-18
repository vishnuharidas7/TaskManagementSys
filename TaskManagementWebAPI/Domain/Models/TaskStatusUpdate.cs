namespace TaskManagementWebAPI.Domain.Models
{
    public class TaskStatusUpdate
    {
        public DateTime DueDate { get; private set; }
        public string Status { get; private set; }

        public void UpdateStatusToOnDue()
        {
            Status = "On Due";
        }
    }
}
