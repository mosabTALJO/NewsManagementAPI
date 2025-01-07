using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialEditedVM
    {
        public int ID { get; set; }
        public string? FinalContent { get; set; }
        public string? EditorNote { get; set; }
        public bool SaveOnly { get; set; }
        public bool SkipProofreading { get; set; }
        public bool SendToDesigner { get; set; }
        public PublishingStrategy? PublishingStrategy { get; set; }
    }
}