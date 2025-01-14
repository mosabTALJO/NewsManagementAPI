namespace NewsManagementAPI.Models
{
    public class NewsSocialMedia
    {
        public int ID { get; set; }
        public int SocialMediaID { get; set; }
        public SocialMedia? SocialMedia { get; set; }
        public int NewsMaterialID { get; set; }
        public NewsMaterial? NewsMaterial { get; set; }
        public required string Url { get; set; }
        public bool Schedule { get; set; }
    }
}