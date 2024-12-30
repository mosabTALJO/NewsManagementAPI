using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.Security.Claims;

namespace NewsManagementAPI.Helpers
{
    public static class UserHelper
    {
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(CreateUserVM userVM, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = userVM.DisplayName,
                UserName = userVM.DisplayName,
                Email = userVM.Email,
                Image = userVM.Image,
                PhoneNumber = userVM.PhoneNumber,
            };
            return await CreateAdminUser(newsUser, userVM.Password, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(string email, string username, string password, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = username,
                UserName = username,
                Email = email,
            };
            return await CreateAdminUser(newsUser, password, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(NewsUser newsUser, string password, UserManager<NewsUser> userManager)
        {
            var result = await userManager.CreateAsync(newsUser, password);
            if (result.Succeeded)
            {
                //await userManager.RemoveClaimsAsync(newsUser, await userManager.GetClaimsAsync(newsUser));
                await userManager.AddClaimsAsync(newsUser, [new Claim(ClaimTypes.Role, NewsUserRole.Admin.GetPolicyNumber())]);
                return (newsUser, null);
            }
            else
            {
                string error = string.Join('\n', result.Errors.Select(x => x.Code + ": " + x.Description));
                return (null, error);
            }
        }
    }
}