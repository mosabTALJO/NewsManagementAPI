using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.Security.Claims;

namespace NewsManagementAPI.Helpers
{
    public class MySeedInitializer
    {
        public static async Task SeedData(string email, string username, string password, UserManager<NewsUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager, email, username, password);
        }
        private static async Task SeedUsers(UserManager<NewsUser> userManager, string email, string username, string password)
        {
            var newsUser = await userManager.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == email);
            if (newsUser == null)
            {
                var result = await UserHelper.CreateUser(email, username, password, userManager);
                //Remove currenct policies
                if (result.User != null)
                {
                    await userManager.RemoveClaimsAsync(result.User, await userManager.GetClaimsAsync(result.User));
                    await userManager.AddClaimsAsync(result.User, result.User.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
                }
            }
            else if (newsUser.Roles.Any(x => x.Role == NewsUserRole.Admin) == false)
            {
                newsUser.Roles.Add(new() { Role = NewsUserRole.Admin });
                await userManager.RemoveClaimsAsync(newsUser, await userManager.GetClaimsAsync(newsUser));
                await userManager.AddClaimsAsync(newsUser, newsUser.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
            }
        }
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = Enum.GetValues<NewsUserRole>();
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.ToString()))
                    await roleManager.CreateAsync(new IdentityRole(role.ToString()));
            }
        }
    }
}