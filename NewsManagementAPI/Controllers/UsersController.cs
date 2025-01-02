using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using NewsManagementAPI.Extentions;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using System.Collections.Generic;
using NewsManagementAPI.ViewModels.UserVMs;

namespace NewsManagementAPI.Controllers
{
    public class UsersController : MyController
    {
        readonly IConfiguration configuration;
        public UsersController(UserManager<NewsUser> userManager, IConfiguration configuration, NewsDbContext newsDbContext) : base(userManager, newsDbContext)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<NewsUserVM[]>>> Get(
            int pageNo = 1,
            int pageSize = 20,
            string? search = null,
            string? roles = null)//comma seperated
        {
            try
            {
                if ((roles == null || roles.Length > 1) && !User.HasRole(NewsUserRole.Admin))
                    return Done<NewsUserVM[]>("You're not an admin!", ResponseCode.Unauthorized);
                var q = userManager.Users.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                    q = q.Where(x => x.DisplayName == search || x.Email == search);
                var claimsQuery = newsDbContext.UserClaims.Where(x => x.ClaimType == ClaimTypes.Role);
                if (!string.IsNullOrEmpty(roles))
                {
                    var rolesArray = roles.Split(',');
                    claimsQuery = claimsQuery.Where(x => rolesArray.Contains(x.ClaimValue));
                }
                var userVMs = await q.OrderBy(x => x.DisplayName).ToNewsUserVMsAsync(claimsQuery, pageNo, pageSize);
                return Done(userVMs);
            }
            catch (Exception ex)
            {
                return Done<NewsUserVM[]>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [Authorize(Policy = UserRoleNames.Admin)]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<ApiResponse<string>>> CreateUser(CreateUserVM userVM)
        {
            try
            {
                var result = await UserHelper.CreateAdminUser(userVM, userManager);
                if (result.User != null)
                    return Done(result.User.Id);
                else
                    return Done<string>(result.Error ?? "Error creating user!", ResponseCode.BadRequest);
            }
            catch (Exception ex)
            {
                return Done<string>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [Authorize(Policy = UserRoleNames.Admin)]
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(UpdateUserVM userVM)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userVM.Id);
                if (user == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (string.IsNullOrEmpty(userVM.Email))
                    return Done<bool>("Email is missing!", ResponseCode.BadRequest);
                if (string.IsNullOrEmpty(userVM.DisplayName))
                    return Done<bool>("Display name is missing!", ResponseCode.BadRequest);
                user.PhoneNumber = userVM.PhoneNumber;
                user.Email = userVM.Email;
                user.UserName = userVM.DisplayName;
                user.DisplayName = userVM.DisplayName;
                user.UserType = userVM.UserType;
                if (!string.IsNullOrEmpty(userVM.NewPassword) && !string.IsNullOrEmpty(userVM.OldPassword) && userVM.NewPassword != userVM.OldPassword)
                {
                    var changePass = await userManager.ChangePasswordAsync(user, userVM.OldPassword, userVM.NewPassword);
                    if (!changePass.Succeeded)
                        return Done<bool>(string.Join(", ", changePass.Errors.Select(x => $"{x.Code}, {x.Description}")), ResponseCode.BadRequest);
                }
                await userManager.UpdateAsync(user);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [Authorize(Policy = UserRoleNames.Admin)]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string userId)
        {
            try
            {
                var result = await userManager.Users.Where(x => x.Id == userId).ExecuteDeleteAsync();
                if (result == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenVM>>> Login([FromBody] LogInVM logInVM)
        {
            try
            {
                var user = await userManager.Users.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email == logInVM.Email);
                if (user == null)
                    return Done<TokenVM>("Email not found!", ResponseCode.NotFound);

                if (!await userManager.CheckPasswordAsync(user, logInVM.Password))
                    return Done<TokenVM>("Incorrect password", ResponseCode.Unauthorized);

                var token = await TokenGenerator.CreateToken(user, userManager, configuration);
                return Done(token);
            }
            catch (Exception ex)
            {
                return Done<TokenVM>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        //[HttpPost("login2")]
        //public async Task<ActionResult<ApiResponse<TokenVM>>> Login2([FromBody] CreateUserVM createUserVM)
        //{
        //    try
        //    {
        //        NewsUser newsUser = new()
        //        {
        //            DisplayName = createUserVM.DisplayName,
        //            Email = createUserVM.Email,
        //            PasswordHash = createUserVM.Password,
        //            Image = createUserVM.Image,
        //            PhoneNumber = createUserVM.PhoneNumber,
        //            Roles = createUserVM.Roles.Select(x => new UserRole { Role = NewsUserRole.Admin }).ToList()
        //        };
        //        var token = await TokenGenerator.CreateToken(newsUser, userManager, configuration);
        //        return Done(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Done<TokenVM>(ex.AllMessages(), ResponseCode.BadRequest);
        //    }
        //}
    }
}