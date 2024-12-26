using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class ReportedNewsVM
    {
        public required string Content { get; set; }
        public NewsMaterialType Type { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public string? Attachments { get; set; }
        public string? Images { get; set; }
        public bool ExtremelyUrgent { get; set; }
    }
}
