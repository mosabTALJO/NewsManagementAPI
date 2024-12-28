using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsManagementAPI.Helpers
{
    public class TokenGenerator
    {
        public static async Task<TokenVM> CreateToken(NewsUser user, UserManager<NewsUser> userManager, IConfiguration configuration)
        {
            var claims = await userManager.GetClaimsAsync(user) ?? [];
            claims.Add(new(ClaimTypes.Name, user.Id));
            foreach (var r in user.Roles)
                claims.Add(new(ClaimTypes.Role, r.Role.GetPolicyNumber()));

            var signinKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));

            var token = new JwtSecurityToken(
              issuer: configuration["Jwt:Site"],
              audience: configuration["Jwt:Site"],
              expires: DateTime.UtcNow.AddDays(60),
              claims: claims,
              signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
            );

            return new TokenVM
            {
                UserID = user.Id,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Roles = user.Roles.Select(x => x.Role).ToList()
            };
        }
        public static string GeneratePassword(int passLength)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz@#$&ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, passLength)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
    }
}
