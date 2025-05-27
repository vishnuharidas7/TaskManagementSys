namespace TaskManagementWebAPI.DTOs
{
    public class AddTaskDTO
    {
        public string taskName { get; set; }
        public int UserId { get; set; }

        public DateTime dueDate { get; set; }

        public string taskDescription { get; set; }

        //public string taskStatus { get; set; }
    }
}
