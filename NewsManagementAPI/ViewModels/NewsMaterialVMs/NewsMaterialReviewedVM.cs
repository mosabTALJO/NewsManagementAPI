using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialReviewedVM
    {
        public int ID { get; set; }
        public required string Content { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public string? ReviewNote { get; set; }
        public bool ExtremelyUrgent { get; set; }
        public bool ContentRequiresConfirmation { get; set; }
        public DateTime? DeadLine { get; set; }
        public List<PublishingStrategy> PublishingStrategies { get; set; } = [];
    }
}