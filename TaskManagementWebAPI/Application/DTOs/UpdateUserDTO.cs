namespace TaskManagementWebAPI.Application.DTOs
{
    public class UpdateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        //public bool IsActive { get; set; }

    }
}
