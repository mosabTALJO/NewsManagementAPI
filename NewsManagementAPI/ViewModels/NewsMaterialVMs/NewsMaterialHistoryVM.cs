using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialHistoryVM
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public required string NewsUserID { get; set; }
        public string? AssignerID { get; set; }
        public DateTime Date { get; set; }
        public NewsMaterialStatus Status { get; set; }
    }
}