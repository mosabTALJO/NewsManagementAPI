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
        public static string GetPolicyNumber(this NewsUserRole p) => ((int)p).ToString();
        public static bool HasPemission(this ClaimsPrincipal user, NewsMaterialStatus status)
        {
            if (user.HasClaim(x => x.Type == NewsUserRole.Admin.GetPolicyNumber()))
                return true;
            return status switch
            {
                NewsMaterialStatus.Reported => user.HasClaim(x => x.Type == NewsUserRole.Reviewer.GetPolicyNumber()),
                NewsMaterialStatus.Reviewing => user.HasClaim(x => x.Type == NewsUserRole.Reviewer.GetPolicyNumber()),
                NewsMaterialStatus.Reviewed => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber()),
                NewsMaterialStatus.AssignedForEditing => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber() || x.Type == NewsUserRole.Editor.GetPolicyNumber()),
                NewsMaterialStatus.Editing => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber() || x.Type == NewsUserRole.Editor.GetPolicyNumber()),
                NewsMaterialStatus.Edited => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber()),
                NewsMaterialStatus.AssignedForProofreading => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber() || x.Type == NewsUserRole.Proofreader.GetPolicyNumber()),
                NewsMaterialStatus.Proofreading => user.HasClaim(x => x.Type == NewsUserRole.EditingManager.GetPolicyNumber() || x.Type == NewsUserRole.Proofreader.GetPolicyNumber()),
                NewsMaterialStatus.Proofreaded => user.HasClaim(x => x.Type == NewsUserRole.DesigningManager.GetPolicyNumber()),
                NewsMaterialStatus.Designing => user.HasClaim(x => x.Type == NewsUserRole.Designer.GetPolicyNumber() || x.Type == NewsUserRole.DesigningManager.GetPolicyNumber()),
                NewsMaterialStatus.Designed => user.HasClaim(x => x.Type == NewsUserRole.PublishingManager.GetPolicyNumber()),
                NewsMaterialStatus.AssignedForPublishing => user.HasClaim(x => x.Type == NewsUserRole.PublishingManager.GetPolicyNumber() || x.Type == NewsUserRole.Publisher.GetPolicyNumber()),
                NewsMaterialStatus.Publishing => user.HasClaim(x => x.Type == NewsUserRole.PublishingManager.GetPolicyNumber() || x.Type == NewsUserRole.Publisher.GetPolicyNumber()),
                NewsMaterialStatus.Published => user.HasClaim(x => x.Type == NewsUserRole.PublishingManager.GetPolicyNumber()),
                _ => false,
            };
        }
        public static bool HasRole(this ClaimsPrincipal user, NewsUserRole role)
            => user.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == role.GetPolicyNumber());
    }
}
