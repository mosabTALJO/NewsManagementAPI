using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsMaterialHistoryVM
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public required string NewsUserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public NewsMaterialStatus Status { get; set; }
    }
}