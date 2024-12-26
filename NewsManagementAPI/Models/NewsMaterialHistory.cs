namespace NewsManagementAPI.Models
{
    public class NewsMaterialHistory
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public NewsMaterial? NewsMaterial { get; set; }
        public required string NewsUserID { get; set; }
        public NewsUser? NewsUser { get; set; }
        public string? AssignerID { get; set; }
        public NewsUser? Assigner { get; set; }
        public DateTime Date { get; set; }
        public NewsMaterialStatus Status { get; set; }
    }
}