namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialPublishedVM
    {
        public int ID { get; set; }
        public List<SocialMediaVM> SocialMedias { get; set; } = [];
    }
    public class SocialMediaVM
    {
        public int SocialMediaID { get; set; }
        public required string Url { get; set; }
    }
}
