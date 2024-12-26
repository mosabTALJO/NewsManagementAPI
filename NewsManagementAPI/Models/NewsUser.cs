using Microsoft.AspNetCore.Identity;

namespace NewsManagementAPI.Models
{
    public class NewsUser : IdentityUser
    {
        public required string DisplayName { get; set; }
        public string? Image { get; set; }
        public List<UserRole> Roles { get; set; } = [];
    }
    public enum NewsUserRole
    {
        Admin = 0,
        Reporter = 1,
        Reviewer = 2,
        Editor = 3,
        EditingManager = 4,
        Proofreader = 5,
        ProofreadingManager = 6,
        Designer = 7,
        DesigningManager = 8,
        Publisher = 9,
        PublishingManager = 10,
    }
}