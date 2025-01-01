using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.UserVMs
{
    public class CreateUserVM
    {
        public required string DisplayName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public NewsUserType UserType { get; set; }
        public List<NewsUserRole> Roles { get; set; } = [];
    }
}