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

namespace NewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : MyController
    {
        readonly IMemoryCache memoryCache;
        readonly IConfiguration configuration;
        public UsersController(UserManager<NewsUser> userManager, IMemoryCache memoryCache, IConfiguration configuration) : base(userManager)
        {
            this.memoryCache = memoryCache;
            this.configuration = configuration;
        }

        [HttpPost("Get")]
        [Authorize(Policy = nameof(NewsUserRole.Admin))]
        public async Task<ActionResult<ApiResponse<NewsUserVM[]>>> Get(int pageNo, int pageSize, string search, [FromBody] List<NewsUserRole> roles)
        {
            try
            {
                var q = userManager.Users;
                if (!string.IsNullOrEmpty(search))
                    q = q.Where(x => x.UserName == search || x.Email == search);
                if (roles.Count > 0)
                    q = q.Where(x => x.Roles.Any(r => roles.Contains(r.Role)));
                return Done(await q.SkipTake(pageNo, pageSize).ToNewsUserVM().ToArrayAsync());
            }
            catch (Exception ex)
            {
                return Done<NewsUserVM[]>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [Authorize(Policy = nameof(NewsUserRole.Admin))]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<ApiResponse<string>>> CreateUser(CreateUserVM userVM)
        {
            try
            {
                NewsUser newsUser = new()
                {
                    DisplayName = userVM.DisplayName,
                    Roles = userVM.Roles.Select(x => new UserRole { Role = x }).ToList(),
                    Email = userVM.Email,
                    Image = userVM.Image,
                    PhoneNumber = userVM.PhoneNumber,
                };
                var result = await userManager.CreateAsync(newsUser, userVM.Password);
                if (result.Succeeded)
                {
                    await userManager.AddClaimsAsync(newsUser, newsUser.Roles.Select(x => new Claim(x.Role.GetPolicyNumber(), x.Role.GetPolicyNumber())));
                    //Mustafa why not like this?
                    //await userManager.AddClaimsAsync(newsUser, newsUser.Roles.Select(x => new Claim(ClaimTypes.Role, x.Role.GetPolicyNumber())));
                    memoryCache.Set(newsUser.Id, newsUser.Roles.Select(x => x.Role.GetPolicyNumber()).ToArray());
                    return Done(newsUser.Id);
                }
                else
                {
                    string error = string.Join('\n', result.Errors.Select(x => x.Code + ": " + x.Description));
                    return Done<string>(error, ResponseCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return Done<string>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        //add update and delte

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenVM>>> Login(string email, string password)
        {
            try
            {
                var user = await userManager.Users.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email == email);
                if (user == null)
                    return Done<TokenVM>("Email not found!", ResponseCode.NotFound);

                if (!await userManager.CheckPasswordAsync(user, password))
                    return Done<TokenVM>("Incorrect password", ResponseCode.Unauthorized);

                var token = await TokenGenerator.CreateToken(user, userManager, configuration);
                return Done(token);
            }
            catch (Exception ex)
            {
                return Done<TokenVM>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }
    }
}