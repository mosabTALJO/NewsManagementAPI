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
        public NewsRegion NewsRegion { get; set; }
        public string? Attachments { get; set; }
        public string? Images { get; set; }
        public string? ReviewNote { get; set; }
        public string? EditorNote { get; set; }
        public NewsMaterialStatus CurrentStatus { get; set; }
        public string? FinalContent { get; set; }
        public string? FinalAttachments { get; set; }
        public bool ExtremelyUrgent { get; set; }
        public bool ContentRequiresConfirmation { get; set; }
        public bool ContentConfirmed { get; set; }
        public string? PublisherNote { get; set; }
        public DateTime? DeadLine { get; set; }
        public virtual List<NewsMaterialHistory> Histories { get; set; } = [];
        public virtual List<NewsMaterialPublishStrat> PublishingStrategies { get; set; } = [];
        public virtual List<NewsSocialMedia> NewsSocialMedias { get; set; } = [];
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
    public enum NewsMaterialStatus
    {
        Draft = -1,
        Reported = 0,
        //Reviewing = 1,
        Reviewed = 2,
        AssignedForEditing = 3,
        Editing = 4,
        Edited = 5,
        AssignedForProofreading = 6,
        Proofreading = 7,
        Proofreaded = 8, //تدقيق لغوي
        AssignedForDesigning = 9,
        Designing = 10,
        Designed = 11,
        AssignedForPublishing = 12,
        //Publishing = 13,
        Published = 14,
        WaitingPublisherConfirmation = 15,
    }
    public enum NewsRegion
    {
        Local = 0,
        International = 1,
    }
}