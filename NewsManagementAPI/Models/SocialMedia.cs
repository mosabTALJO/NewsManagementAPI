namespace NewsManagementAPI.Models
{
    public class SocialMedia
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public List<NewsSocialMedia> NewsSocialMedias { get; set; } = [];
    }
}