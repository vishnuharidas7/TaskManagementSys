namespace TaskManagementWebAPI.Domain.Models
{
    public class TaskRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; } // New, OnDue, Completed
        public DateTime DueDate { get; set; }

    }
}
