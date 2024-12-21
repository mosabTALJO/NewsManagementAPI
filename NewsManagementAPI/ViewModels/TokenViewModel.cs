using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class TokenViewModel
    {
        public required string userID { get; set; }
        public required string token { get; set; }
        public DateTime expiration { get; set; }
        public string? UserName { get; set; }
        public required string DisplayName { get; set; }
        public NewsUserRole Role { get; set; }
    }
}