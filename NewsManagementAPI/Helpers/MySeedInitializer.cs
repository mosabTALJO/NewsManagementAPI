using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using System.Security.Claims;

namespace NewsManagementAPI.Helpers
{
    public class MySeedInitializer
    {
        public static async Task SeedData(string email, string displayName, string password, UserManager<NewsUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager, email, displayName, password);
        }
        private static async Task SeedUsers(UserManager<NewsUser> userManager, string email, string displayName, string password)
        {
            //var qwe = await newsDbContext.Users.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Email == email);
            //var newsUser = await userManager.Users/*.Include(x => x.Roles)*/.Include(x => x.Claims).FirstOrDefaultAsync(x => x.Email == email);
            var newsUser = await userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (newsUser == null)
                await UserHelper.CreateAdminUser(email, displayName, password, userManager);
            else if ((await userManager.GetClaimsAsync(newsUser)).Any(x => x.Type == ClaimTypes.Role && x.Value == NewsUserRole.Admin.GetPolicyNumber()) == false)
            {
                await userManager.RemoveClaimsAsync(newsUser, await userManager.GetClaimsAsync(newsUser));
                await userManager.AddClaimsAsync(newsUser,[ new Claim(ClaimTypes.Role, NewsUserRole.Admin.GetPolicyNumber())]);
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