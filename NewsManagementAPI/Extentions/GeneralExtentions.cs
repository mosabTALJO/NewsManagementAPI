using Microsoft.AspNetCore.Identity;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using System.Security.Claims;

namespace NewsManagementAPI.Extentions
{
    public static class GeneralExtentions
    {
        public static string AllMessages(this Exception ex) => $"{ex.Message}___{ex.InnerException?.Message}";
        public static string GetPolicyNumber(this NewsUserRole p) => ((int)p).ToString();
        public static bool HasRole(this ClaimsPrincipal user, NewsUserRole role) 
            => user.HasClaim(x => x.Type == ClaimTypes.Role && x.Value == role.GetPolicyNumber());
        public static bool HasRole(this IEnumerable<IdentityUserClaim<string>> claims, NewsUserRole role) 
            => claims.Any(x => x.ClaimType == ClaimTypes.Role && x.ClaimValue == role.GetPolicyNumber());
        public static bool IsManager(this ClaimsPrincipal user)
        {
            return user.HasRole(NewsUserRole.EditingManager)
                || user.HasRole(NewsUserRole.ProofreadingManager)
                || user.HasRole(NewsUserRole.DesigningManager)
                || user.HasRole(NewsUserRole.PublishingManager)
                || user.HasRole(NewsUserRole.Reviewer)
                || user.HasRole(NewsUserRole.Admin);
        }
        public static bool HasPemission(this ClaimsPrincipal user, NewsMaterialStatus status)
        {
            if (user.HasRole(NewsUserRole.Admin))
                return true;
            return status switch
            {
                NewsMaterialStatus.Reported => user.HasRole(NewsUserRole.Reviewer),
                NewsMaterialStatus.Reviewing => user.HasRole(NewsUserRole.Reviewer),
                NewsMaterialStatus.Reviewed => user.HasRole(NewsUserRole.EditingManager),
                NewsMaterialStatus.AssignedForEditing => user.HasRole(NewsUserRole.EditingManager) || user.HasRole(NewsUserRole.Editor),
                NewsMaterialStatus.Editing => user.HasRole(NewsUserRole.EditingManager) || user.HasRole(NewsUserRole.Editor),
                NewsMaterialStatus.Edited => user.HasRole(NewsUserRole.ProofreadingManager),
                NewsMaterialStatus.AssignedForProofreading => user.HasRole(NewsUserRole.ProofreadingManager) || user.HasRole(NewsUserRole.Proofreader),
                NewsMaterialStatus.Proofreading => user.HasRole(NewsUserRole.ProofreadingManager) || user.HasRole(NewsUserRole.Proofreader),
                NewsMaterialStatus.Proofreaded => user.HasRole(NewsUserRole.DesigningManager),
                NewsMaterialStatus.Designing => user.HasRole(NewsUserRole.Designer) || user.HasRole(NewsUserRole.DesigningManager),
                NewsMaterialStatus.Designed => user.HasRole(NewsUserRole.PublishingManager),
                NewsMaterialStatus.AssignedForPublishing => user.HasRole(NewsUserRole.PublishingManager) || user.HasRole(NewsUserRole.Publisher),
                NewsMaterialStatus.Publishing => user.HasRole(NewsUserRole.PublishingManager) || user.HasRole(NewsUserRole.Publisher),
                NewsMaterialStatus.Published => user.HasRole(NewsUserRole.PublishingManager),
                _ => false,
            };
        }
    }
}
