namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialEditedVM
    {
        public int ID { get; set; }
        public string? FinalContent { get; set; }
        public string? EditorNote { get; set; }
        public bool Done { get; set; }
        public bool SkipProofreading { get; set; }
    }
}