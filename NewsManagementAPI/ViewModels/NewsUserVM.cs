using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsUserVM
    {
        public string Id { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public List<NewsUserRole> Roles { get; set; } = [];
    }
}