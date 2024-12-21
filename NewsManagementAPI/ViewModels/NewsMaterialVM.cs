using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialVM
    {
        public int ID { get; set; }
        public NewsUserVM? CurrentUser { get; set; }
        public required string Content { get; set; }
        public NewsMaterialType Type { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public DateTime ReportedDate { get; set; }
        public string? Attachments { get; set; }
        public string? Images { get; set; }
        public string? ReviewNote { get; set; }
        public string? EditorNote { get; set; }
        public string? PublishingStrategies { get; set; }
        public NewsMaterialStatus CurrentStatus { get; set; }
        public string? FinalContent { get; set; }
        public string? FinalAttachments { get; set; }
        public virtual List<NewsMaterialHistoryVM>? Histories { get; set; }
    }
}