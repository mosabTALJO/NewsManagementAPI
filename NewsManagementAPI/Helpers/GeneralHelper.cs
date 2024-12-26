using NewsManagementAPI.Models;

namespace NewsManagementAPI.Helpers
{
    public static class GeneralHelper
    {
        public static string GetPolicyNumber(this NewsUserRole p) => ((int)p).ToString();
        public static bool HasPemission(this NewsUser newsUser, NewsMaterialStatus status) => HasPemission(newsUser.Roles, status);
        public static bool HasPemission(this List<UserRole> userRoles, NewsMaterialStatus status)
        {
            if (userRoles.Any(x => x.Role == NewsUserRole.Admin))
                return true;
            return status switch
            {
                NewsMaterialStatus.Reported => userRoles.Any(x => x.Role == NewsUserRole.Reviewer),
                NewsMaterialStatus.Reviewing => userRoles.Any(x => x.Role == NewsUserRole.Reviewer),
                NewsMaterialStatus.Reviewed => userRoles.Any(x => x.Role == NewsUserRole.EditingManager),
                NewsMaterialStatus.AssignedForEditing => userRoles.Any(x => x.Role == NewsUserRole.EditingManager || x.Role == NewsUserRole.Editor),
                NewsMaterialStatus.Editing => userRoles.Any(x => x.Role == NewsUserRole.EditingManager || x.Role == NewsUserRole.Editor),
                NewsMaterialStatus.Edited => userRoles.Any(x => x.Role == NewsUserRole.EditingManager),
                NewsMaterialStatus.AssignedForProofreading => userRoles.Any(x => x.Role == NewsUserRole.EditingManager || x.Role == NewsUserRole.Proofreader),
                NewsMaterialStatus.Proofreading => userRoles.Any(x => x.Role == NewsUserRole.EditingManager || x.Role == NewsUserRole.Proofreader),
                NewsMaterialStatus.Proofreaded => userRoles.Any(x => x.Role == NewsUserRole.DesigningManager),
                NewsMaterialStatus.Designing => userRoles.Any(x => x.Role == NewsUserRole.Designer || x.Role == NewsUserRole.DesigningManager),
                NewsMaterialStatus.Designed => userRoles.Any(x => x.Role == NewsUserRole.PublishingManager),
                NewsMaterialStatus.AssignedForPublishing => userRoles.Any(x => x.Role == NewsUserRole.PublishingManager || x.Role == NewsUserRole.Publisher),
                NewsMaterialStatus.Publishing => userRoles.Any(x => x.Role == NewsUserRole.PublishingManager || x.Role == NewsUserRole.Publisher),
                NewsMaterialStatus.Published => userRoles.Any(x => x.Role == NewsUserRole.PublishingManager),
                _ => false,
            };
        }
    }
}