namespace NewsManagementAPI.ViewModels.NewsMaterialVMs
{
    public class NewsMaterialLogVM
    {
        public int ID { get; set; }
        public int NewsMaterialID { get; set; }
        public required string NewsUserID { get; set; }
        public required string NewsUserDisplayName { get; set; }
        public required string Log { get; set; }
        public DateTime Date { get; set; }
        public string[] Sentences => Log.Split('\n');
    }
}