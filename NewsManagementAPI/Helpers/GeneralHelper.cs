using NewsManagementAPI.Models;

namespace NewsManagementAPI.Helpers
{
    public static class GeneralHelper
    {
        public static string GetPolicyNumber(this NewsUserRole p) => ((int)p).ToString();
    }
}
