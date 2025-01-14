namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialPublishedVM
    {
        public int ID { get; set; }
        public List<NewsSocialMediaVM> SocialMedias { get; set; } = [];
    }
    public class NewsSocialMediaVM
    {
        public int SocialMediaID { get; set; }
        public required string Url { get; set; }
        public bool Schedule { get; set; }
    }
}
