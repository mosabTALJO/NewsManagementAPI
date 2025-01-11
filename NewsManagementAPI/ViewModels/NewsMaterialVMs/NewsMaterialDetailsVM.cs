using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels.UserVMs;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialDetailsVM
    {
        public int ID { get; set; }
        public NewsUserLiteVM? CurrentUser { get; set; }
        public required string Content { get; set; }
        public NewsMaterialType Type { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public NewsRegion NewsRegion { get; set; }
        public DateTime ReportedDate { get; set; }
        public string? Attachments { get; set; }
        public string? Images { get; set; }
        public string? ReviewNote { get; set; }
        public string? EditorNote { get; set; }
        public bool ExtremelyUrgent { get; set; }
        public bool ContentRequiresConfirmation { get; set; }
        public List<PublishingStrategy> PublishingStrategies { get; set; } = [];
        public NewsMaterialStatus CurrentStatus { get; set; }
        public string? FinalContent { get; set; }
        public string? FinalAttachments { get; set; }
        public DateTime? DeadLine { get; set; }
        public bool SendToDesigner { get; set; }
        public PublishingStrategy FinalPublishingStrategy { get; set; }
        public List<NewsMaterialHistoryVM> Histories { get; set; } = [];
        public List<NewsMaterialLogVM> Logs { get; set; } = [];
    }
}
