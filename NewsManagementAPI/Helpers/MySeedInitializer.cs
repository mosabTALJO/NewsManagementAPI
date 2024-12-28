using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.Security.Claims;

namespace NewsManagementAPI.Helpers
{
    public class MySeedInitializer
    {
        public static void SeedData(UserManager<NewsUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMemoryCache memoryCache
            )
        {
            SeedRoles(roleManager).Wait();
            SeedUsers(userManager).Wait();
            RefreshMemoryCache(userManager, memoryCache).Wait();
        }

        private static async Task SeedUsers(UserManager<NewsUser> userManager)
        {
            var user = userManager.Users.FirstOrDefault(x => x.Email == "admin@ecommerce.com");
            if (user is null)
            {
                user = new NewsUser
                {
                    UserName = "adminecommerce",
                    DisplayName = "Admin",
                    Email = "admin@ecommerce.com",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Roles = [new() { Role = NewsUserRole.Admin }]
                };
                await userManager.CreateAsync(user, "P@ssw0rd");
            }
            //Remove currenct policies
            await userManager.RemoveClaimsAsync(user, await userManager.GetClaimsAsync(user));
            await userManager.AddClaimsAsync(user, user.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
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
        private static async Task RefreshMemoryCache(UserManager<NewsUser> userManager, IMemoryCache memoryCache)
        {
            var us = userManager.Users.ToList();
            foreach (var u in us)
            {
                var cs = await userManager.GetClaimsAsync(u);
                memoryCache.Remove(u.Id);
                memoryCache.Set(u.Id, cs.Select(e => e.Type).ToArray());
            }
        }
    }
}