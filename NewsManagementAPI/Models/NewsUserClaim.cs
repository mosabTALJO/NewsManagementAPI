using Microsoft.AspNetCore.Identity;

namespace NewsManagementAPI.Models
{
    public class NewsUserClaim : IdentityUserClaim<string>
    {
        public new string UserId { get; set; }
        public NewsUser? User { get; set; }
    }
}