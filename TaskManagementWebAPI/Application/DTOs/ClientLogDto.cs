namespace TaskManagementWebAPI.Application.DTOs
{
    public class ClientLogDto
    {
        public string Level { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string Timestamp { get; set; }
        public string Url { get; set; }
    }
}
