using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels.UserVMs
{
    public class NewsUserLiteVM
    {
        public string Id { get; set; }
        public required string DisplayName { get; set; }
        public ReporterType ReporterType { get; set; }
    }
}