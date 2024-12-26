namespace NewsManagementAPI.Models
{
    public class UserRole
    {
        public int ID { get; set; }
        public required string NewsUserID { get; set; }
        public NewsUser? NewsUser { get; set; }
        public NewsUserRole Role { get; set; }
    }
}