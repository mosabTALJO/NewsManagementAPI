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
        public static async Task<(NewsUser? User, string? Error)> CreateUser(CreateUserVM userVM, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = userVM.DisplayName,
                UserName = userVM.DisplayName,
                Roles = userVM.Roles.Select(x => new UserRole { Role = x }).ToList(),
                Email = userVM.Email,
                Image = userVM.Image,
                PhoneNumber = userVM.PhoneNumber,
            };
            return await CreateUser(newsUser, userVM.Password, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateUser(string email, string username, string password, UserManager<NewsUser> userManager)
        {
            NewsUser newsUser = new()
            {
                DisplayName = username,
                UserName = username,
                Roles = [new() { Role = NewsUserRole.Admin }],
                Email = email,
            };
            return await CreateUser(newsUser, password, userManager);
        }
        public static async Task<(NewsUser? User, string? Error)> CreateUser(NewsUser newsUser, string password, UserManager<NewsUser> userManager)
        {
            var result = await userManager.CreateAsync(newsUser, password);
            if (result.Succeeded)
            {
                await userManager.AddClaimsAsync(newsUser, newsUser.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
                //Mustafa why not like this?
                //await userManager.AddClaimsAsync(newsUser, newsUser.Roles.Select(x => new Claim(ClaimTypes.Role, x.Role.GetPolicyNumber())));
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