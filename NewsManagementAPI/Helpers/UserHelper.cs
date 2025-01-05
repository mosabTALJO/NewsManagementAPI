using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels.UserVMs;
using System.Security.Claims;

namespace NewsManagementAPI.Helpers
{
    public static class UserHelper
    {
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(string email, string displayName, string password, UserManager<NewsUser> userManager)
        {
            CreateUserVM userVM = new()
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                Roles = [NewsUserRole.Admin]
            };
            return await CreateAdminUser(userVM, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(CreateUserVM userVM, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = userVM.DisplayName,
                UserName = userVM.Email,
                Email = userVM.Email,
                Image = userVM.Image,
                PhoneNumber = userVM.PhoneNumber,
                ReporterType = userVM.ReporterType,
            };
            var result = await userManager.CreateAsync(newsUser, userVM.Password);
            if (result.Succeeded)
            {
                await userManager.AddClaimsAsync(newsUser, userVM.Roles.Select(x => new Claim(ClaimTypes.Role, x.GetPolicyNumber())));
                return (newsUser, null);
            }
            else
            {
                string error = string.Join('\n', result.Errors.Select(x => x.Code + ": " + x.Description));
                return (null, error);
            }
        }
        public static readonly Dictionary<NewsUserRole, NewsMaterialStatus[]> RoleAllowedStatuses = new()
        {
            { NewsUserRole.Admin, Enum.GetValues<NewsMaterialStatus>() },
            { NewsUserRole.Reporter, [NewsMaterialStatus.Draft] },
            { NewsUserRole.Reviewer, [NewsMaterialStatus.Reported] },
            { NewsUserRole.Editor, [NewsMaterialStatus.AssignedForEditing, NewsMaterialStatus.Editing] },
            { NewsUserRole.EditingManager, [NewsMaterialStatus.Reviewed, NewsMaterialStatus.AssignedForEditing, NewsMaterialStatus.Editing] },
            { NewsUserRole.Proofreader, [NewsMaterialStatus.AssignedForProofreading, NewsMaterialStatus.Proofreading] },
            { NewsUserRole.ProofreadingManager, [NewsMaterialStatus.AssignedForEditing, NewsMaterialStatus.Editing, NewsMaterialStatus.Edited] },
            { NewsUserRole.Designer, [NewsMaterialStatus.AssignedForDesigning, NewsMaterialStatus.Designing] },
            { NewsUserRole.DesigningManager, [NewsMaterialStatus.AssignedForDesigning, NewsMaterialStatus.Designing, NewsMaterialStatus.Proofreaded] },
            { NewsUserRole.Publisher, [NewsMaterialStatus.AssignedForPublishing] },
            { NewsUserRole.PublishingManager, [NewsMaterialStatus.AssignedForPublishing, NewsMaterialStatus.Designed, NewsMaterialStatus.WaitingPublisherConfirmation] },
        };
    }
}