namespace NewsManagementAPI.Models
{
    public class NewsMaterial
    {
        public int ID { get; set; }
        public string? CurrentUserID { get; set; }
        public NewsUser? CurrentUser { get; set; }
        public required string Content { get; set; }
        public DateTime ReportedDate { get; set; }
        public NewsMaterialType Type { get; set; }
        public Priority Priority { get; set; }
        public Urgency Urgency { get; set; }
        public string? Attachments { get; set; }
        public string? Images { get; set; }
        public string? ReviewNote { get; set; }
        public string? EditorNote { get; set; }
        public string? PublishingStrategies { get; set; }
        //public List<PublishingStrategy> PublishingStrategies { get; set; } = [];
        public NewsMaterialStatus CurrentStatus { get; set; }
        public string? FinalContent { get; set; }
        public string? FinalAttachments { get; set; }
        public virtual List<NewsMaterialHistory>? Histories { get; set; }
    }
    public enum NewsMaterialType
    {
        Public = 0,
        Private = 1,
        Exclusive = 2
    }
    public enum Priority
    {
        NotImportant = 0,
        Important = 1,
        VeryImportant = 2
    }
    public enum Urgency
    {
        NotUrgent = 0,
        Urgent = 1,
        VeryUrgent = 2
    }
    public enum PublishingStrategy
    {
        Text = 0,
        Visual = 1,
        Images = 2,
        Audio = 3,
        Interactive = 4
    }
    public enum NewsMaterialStatus
    {
        Reported = 0,
        Reviewing = 1,
        Reviewed = 2,
        Editing = 3,
        Edited = 4,
        Designing = 5,
        Designed = 6,
        Published = 7
    }
}