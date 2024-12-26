using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using System.Security.Claims;

namespace NewsManagementAPI.Extentions
{
    public static class GeneralExtentions
    {
        public static string AllMessages(this Exception ex) => $"{ex.Message}___{ex.InnerException?.Message}";
        public static bool IsManager(this ClaimsPrincipal user)
        {
            return user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber()
                                   || x.Type == NewsUserRole.ProofreadingManager.GetPolicyNumber()
                                   || x.Type == NewsUserRole.DesigningManager.GetPolicyNumber()
                                   || x.Type == NewsUserRole.PublishingManager.GetPolicyNumber()
                                   || x.Type == NewsUserRole.Admin.GetPolicyNumber());
        }
    }
}
