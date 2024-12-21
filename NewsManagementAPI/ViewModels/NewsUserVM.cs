using NewsManagementAPI.Models;

namespace NewsManagementAPI.ViewModels
{
    public class NewsUserVM
    {
        public string Id { get; set; }
        public required string DisplayName { get; set; }
    }
}