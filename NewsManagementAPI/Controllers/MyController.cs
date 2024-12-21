using Microsoft.AspNetCore.Mvc;
using NewsManagementAPI.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : Controller
    {
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
