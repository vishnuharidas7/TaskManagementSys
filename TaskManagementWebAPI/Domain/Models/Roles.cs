using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Domain.Models
{
    public class Roles
    {
        [Key]
        public int RoleId { get; set; }

        [Required,MaxLength(100)]
        public string RoleName { get; set; }

        //Navigation property
        public ICollection<Users> User { get; set; }

    }
}
