﻿namespace TaskManagementWebAPI.DTOs
{
    public class ViewUserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public bool Status { get; set; }
    }
}
