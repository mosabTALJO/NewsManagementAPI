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
    public class UsersController : ControllerBase
    {
        readonly UserManager<NewsUser> userManager;
        readonly IMemoryCache memoryCache;
        readonly IConfiguration configuration;
        public UsersController(UserManager<NewsUser> userManager, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.memoryCache = memoryCache;
            this.configuration = configuration;
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
                    Role = userVM.Role,
                    Email = userVM.Email,
                    Image = userVM.Image,
                    PhoneNumber = userVM.PhoneNumber,
                };
                var result = await userManager.CreateAsync(newsUser, userVM.Password);
                if (result.Succeeded)
                {
                    await userManager.AddClaimsAsync(newsUser, [new Claim(userVM.Role.GetPolicyNumber(), userVM.Role.GetPolicyNumber())]);
                    memoryCache.Set(newsUser.Id, userVM.Role.GetPolicyNumber());
                    return Ok(ApiResponse.CreateSuccess(newsUser.Id));
                }
                else
                {
                    string error = string.Join('\n', result.Errors.Select(x => x.Code + ": " + x.Description));
                    return BadRequest(ApiResponse.CreateFailure(error));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.AllMessages());
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenViewModel>>> Login(string email, string password)
        {
            try
            {
                var user = await userManager.Users.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email == email);
                if (user == null)
                    return NotFound("Email not found!");

                if (!await userManager.CheckPasswordAsync(user, password))
                    return Unauthorized("Incorrect password");

                var token = await TokenGenerator.CreateToken(user, userManager, configuration);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.AllMessages());
            }
        }
    }
}