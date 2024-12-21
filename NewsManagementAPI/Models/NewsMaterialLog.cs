namespace NewsManagementAPI.Models
{
    public class NewsMaterialLog
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public NewsMaterial? NewsMaterial { get; set; }
        public required string NewsUserID { get; set; }
        public NewsUser? NewsUser { get; set; }
        public required string Log { get; set; }
    }
}