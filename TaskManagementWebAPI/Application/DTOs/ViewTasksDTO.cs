namespace TaskManagementWebAPI.Application.DTOs
{
    public class ViewTasksDTO
    {

        public int taskId { get; set; }
        public string taskName { get; set; }

        public int userId { get; set; }

        public string userName { get; set; }
        public string taskDescription { get; set; }
        public DateTime dueDate { get; set; }
        public string taskStatus { get; set; }

        public string priority { get; set; }
        public string taskType { get; set; }

        public string referenceId { get; set; }

    }
}
