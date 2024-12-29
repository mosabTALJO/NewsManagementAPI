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
        public static void SeedData(UserManager<NewsUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager).Wait();
            SeedUsers(userManager).Wait();
        }
        public static async Task SeedDevData(UserManager<NewsUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            var newsUser = await userManager.Users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == "mosabtaljo@gmail.com");
            if (newsUser == null)
            {
                var result = await UserHelper.CreateUser("mosabtaljo@gmail.com", "mosab", "123456lolo", userManager);
                //Remove currenct policies
                if (result.User != null)
                {
                    await userManager.RemoveClaimsAsync(result.User, await userManager.GetClaimsAsync(result.User));
                    await userManager.AddClaimsAsync(result.User, result.User.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
                }
            }
        }
        private static async Task SeedUsers(UserManager<NewsUser> userManager)
        {
            if (await userManager.Users.AnyAsync(x => x.Email == "admin@ecommerce.com") == false)
            {
                var result = await UserHelper.CreateUser("admin@ecommerce.com", "adminecommerce", "P@ssw0rd", userManager);
                //Remove currenct policies
                if (result.User != null)
                {
                    await userManager.RemoveClaimsAsync(result.User, await userManager.GetClaimsAsync(result.User));
                    await userManager.AddClaimsAsync(result.User, result.User.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
                }
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