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
        public async Task<ActionResult<ApiResponse<List<NewsUserVM>>>> Get(
            int pageNo = 1,
            int pageSize = 20,
            string? search = null,
            string? roles = null)//comma seperated
        {
            try
            {
                if ((roles == null || roles.Length > 1) && !User.HasRole(NewsUserRole.Admin))
                    return Done<List<NewsUserVM>>("You're not an admin!", ResponseCode.Unauthorized);
                var q = userManager.Users;
                if (!string.IsNullOrEmpty(search))
                    q = q.Where(x => x.UserName == search || x.Email == search);
                if (!string.IsNullOrEmpty(roles))
                {
                    var rolesArray = roles.Split(',');
                    q = from u in q
                        from r in newsDbContext.UserClaims.Where(x => x.UserId == u.Id && x.ClaimType == ClaimTypes.Role)
                        where rolesArray.Contains(r.ClaimValue)
                        select u;
                }
                var users = await q.SkipTake(pageNo, pageSize).ToArrayAsync();
                List<NewsUserVM> userVMs = [];
                foreach (var user in users)
                {
                    userVMs.Add(new NewsUserVM
                    {
                        DisplayName = user.DisplayName,
                        Email = user.Email!,
                        Image = user.Image,
                        PhoneNumber = user.PhoneNumber,
                        Roles = (await userManager.GetClaimsAsync(user)).Where(x => x.Type == ClaimTypes.Role).Select(x => Enum.Parse<NewsUserRole>(x.Value)).ToList()
                    });
                }
                return Done(userVMs);
            }
            catch (Exception ex)
            {
                return Done<List<NewsUserVM>>(ex.AllMessages(), ResponseCode.BadRequest);
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

        //add update and delte

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