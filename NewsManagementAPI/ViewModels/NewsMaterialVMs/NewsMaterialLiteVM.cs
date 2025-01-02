using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels.UserVMs;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialLiteVM
    {
        public int ID { get; set; }
        public NewsUserLiteVM? CurrentUser { get; set; }
        public required string Content { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public DateTime ReportedDate { get; set; }
        public bool ExtremelyUrgent { get; set; }
        public bool ContentRequiresConfirmation { get; set; }
        public NewsMaterialStatus CurrentStatus { get; set; }
        public DateTime? DeadLine { get; set; }
    }
}
