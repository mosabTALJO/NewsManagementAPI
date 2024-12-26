namespace NewsManagementAPI.Models
{
    public class NewsMaterialPublishStrat
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public NewsMaterial? NewsMaterial { get; set; }
        public PublishingStrategy PublishingStrategy { get; set; }
    }
    public enum PublishingStrategy
    {
        Text = 0,
        Visual = 1,
        Images = 2,
        Audio = 3,
        Interactive = 4
    }
}