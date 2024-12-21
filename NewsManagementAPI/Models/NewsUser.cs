using Microsoft.AspNetCore.Identity;

namespace NewsManagementAPI.Models
{
    public class NewsUser : IdentityUser
    {
        //public int ID { get; set; }
        public required string DisplayName { get; set; }
        //public required string Email { get; set; }
        //public required string Password { get; set; }
        //public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public NewsUserRole Role { get; set; }
    }
    public enum NewsUserRole
    {
        Admin = 0,
        Reporter = 1,
        Reviewer = 2,
        Editor = 3,
        Designer = 4,
        Publisher = 5,
    }
}