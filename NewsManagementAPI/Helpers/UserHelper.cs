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
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(string email, string username, string password, UserManager<NewsUser> userManager)
        {
            CreateUserVM userVM = new()
            {
                Email = email,
                Password = password,
                DisplayName = username,
                Roles = [NewsUserRole.Admin]
            };
            return await CreateAdminUser(userVM, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateAdminUser(CreateUserVM userVM, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = userVM.DisplayName,
                UserName = userVM.DisplayName,
                Email = userVM.Email,
                Image = userVM.Image,
                PhoneNumber = userVM.PhoneNumber,
                UserType = userVM.UserType,
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
    }
}