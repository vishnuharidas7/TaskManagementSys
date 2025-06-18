namespace TaskManagementWebAPI.Application.DTOs
{
    public class NotificationDTO
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskStatus { get; set; }

        public string UserName { get; set; }
    }
}
