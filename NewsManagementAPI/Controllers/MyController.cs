using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;

namespace NewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : Controller
    {
        protected readonly UserManager<NewsUser> userManager;
        public MyController(UserManager<NewsUser> userManager)
        {
            this.userManager = userManager;
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
