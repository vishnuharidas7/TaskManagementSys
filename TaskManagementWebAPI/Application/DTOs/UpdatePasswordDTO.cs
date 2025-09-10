namespace TaskManagementWebAPI.Application.DTOs
{
    public class UpdatePasswordDTO
    {
        public int? id { get; set; }
        public string curpswd { get; set; }
        public string newpswd { get; set; }   
        public string confrmNewpswd { get; set; } 
    }
}
