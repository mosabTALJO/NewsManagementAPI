using Microsoft.AspNetCore.Identity;

namespace NewsManagementAPI.Models
{
    public class NewsUser : IdentityUser
    {
        public required string DisplayName { get; set; }
        public string? Image { get; set; }
        public ReporterType ReporterType { get; set; }
    }
    public enum ReporterType
    {
        Observer = 0,
        Reporter = 1,
        volunteer = 2,
        TeamMember = 3,
        OfficailPerson = 4
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
    public static class UserRoleNames
    {
        public const string Admin = "0";
        public const string Reporter = "1";
        public const string Reviewer = "2";
        public const string Editor = "3";
        public const string EditingManager = "4";
        public const string Proofreader = "5";
        public const string ProofreadingManager = "6";
        public const string Designer = "7";
        public const string DesigningManager = "8";
        public const string Publisher = "9";
        public const string PublishingManager = "10";
    }
}