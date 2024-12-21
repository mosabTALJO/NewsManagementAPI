using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialEditedVM
    {
        public int ID { get; set; }
        public string? PublishingStrategies { get; set; }
        public string? FinalContent { get; set; }
        public string? EditorNote { get; set; }
        public bool Done { get; set; }
    }
}