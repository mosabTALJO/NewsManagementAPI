namespace NewsManagementAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public NewsUser User { get; set; }
        public string? Topic { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? Image { get; set; }
        public string? Url { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }
}