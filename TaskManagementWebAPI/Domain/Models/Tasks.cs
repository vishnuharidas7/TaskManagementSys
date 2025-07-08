using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Domain.Models
{
    [Index(nameof(referenceId), IsUnique = true)]
    public class Tasks
    {
        [Key]
        public int taskId { get; set; }

        [Required]
        public string taskName { get; set; }


        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        public string taskDescription { get; set; }

        public string taskStatus { get; set; } = "New";

        public DateTime createdDate { get; set; } = DateTime.Now;

        public DateTime dueDate { get; set; }

        public string priority { get; set; }

        public int createdBy { get; set; }

        public string taskState { get; set; } = "New";

        public string taskType { get; set; }

        public string referenceId { get; set; }

        public void UpdateStateToDue()
        {
            taskState = "Due";
        }

        public void UpdateStateToOverDue()
        {
            taskState = "OverDue";
        }

    }
}
