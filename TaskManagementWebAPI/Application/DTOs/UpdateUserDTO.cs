namespace TaskManagementWebAPI.Application.DTOs
{
    public class UpdateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        //public string Password { get; set; }

        //public bool IsActive { get; set; }

    }
}
