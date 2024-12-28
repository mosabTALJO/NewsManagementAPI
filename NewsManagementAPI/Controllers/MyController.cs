using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsManagementAPI.Data;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;

namespace NewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : Controller
    {
        protected readonly UserManager<NewsUser> userManager;
        protected readonly NewsDbContext newsDbContext;
        public MyController(UserManager<NewsUser> userManager, NewsDbContext newsDbContext)
        {
            this.userManager = userManager;
            this.newsDbContext = newsDbContext;
        }
        public ActionResult<ApiResponse<T>> Done<T>(T data) => Ok(new ApiResponse<T>() { Data = data });
        public ActionResult<ApiResponse<T>> Done<T>(string error, ResponseCode code)
        {
            return code switch
            {
                ResponseCode.NotFound => NotFound(new ApiResponse<T>() { Errors = [error] }),
                ResponseCode.Unauthorized => Unauthorized(new ApiResponse<T>() { Errors = [error] }),
                _ => BadRequest(new ApiResponse<T>() { Errors = [error] }),
            };
        }
    }
}
