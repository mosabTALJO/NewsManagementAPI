using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.UserVMs
{
    public class UpdateUserVM
    {
        public required string Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Image { get; set; }
        public ReporterType UserType { get; set; }
        public List<NewsUserRole> Roles { get; set; } = [];
    }
}