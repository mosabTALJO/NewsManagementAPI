namespace NewsManagementAPI.Models
{
    public class NewsMaterialHistory
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public NewsMaterial? NewsMaterial { get; set; }
        public required string NewsUserID { get; set; }
        public NewsUser? NewsUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public NewsMaterialStatus Status { get; set; }
    }
}