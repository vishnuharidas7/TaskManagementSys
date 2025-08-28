using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Domain.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int RoleID { get; set; }


        [ForeignKey("RoleID")]
        public Roles? Role { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }


        [Required, MaxLength(100)]
        public string? UserName { get; set; }


        [Required, MaxLength(100)]
        public string? Password { get; set; }


        [Required, MaxLength(100)]

        public string? Email { get; set; }

        [Required, MaxLength(10)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;

        [Required]
        public string gender { get; set; }


        // Additional fields for token handling
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public void UpdatePassword(string newHashedPassword)
        {
            if (string.IsNullOrWhiteSpace(newHashedPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(newHashedPassword));

            Password = newHashedPassword;
        }

    }
}
