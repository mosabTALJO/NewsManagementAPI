using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialReviewedVM
    {
        public int ID { get; set; }
        public required string Content { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public string? ReviewNote { get; set; }
    }
}