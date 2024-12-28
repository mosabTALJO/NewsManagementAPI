using Microsoft.AspNetCore.Identity;

namespace NewsManagementAPI.Models
{
    public class UserRole : IdentityRole
    {
        public string NewsUserID { get; set; }
        public NewsUser? NewsUser { get; set; }
        public NewsUserRole Role { get; set; }
    }
}