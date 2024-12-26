using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class TokenVM
    {
        public required string UserID { get; set; }
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string? UserName { get; set; }
        public required string DisplayName { get; set; }
        public List<NewsUserRole> Roles { get; set; } = [];
    }
}