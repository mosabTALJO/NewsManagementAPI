using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsManagementAPI.Data;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.Services;

namespace NewsManagementAPI.Controllers
{
    public class FilesController : MyController
    {
        private readonly IFileService fileService;
        public FilesController(UserManager<NewsUser> userManager, NewsDbContext newsDbContext, IFileService fileService) : base(userManager, newsDbContext)
        {
            this.fileService = fileService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> Post()
        {
            var newFileName = await fileService.CreateNewFile(Request.Form.Files.ToArray().First());
            if (newFileName == null)
                return Done<string>("Error while uploading file", ResponseCode.BadRequest);
            return Done(newFileName);
        }

        [HttpPost("Post2")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> Post2(IFormFile file)
        {
            var newFileName = await fileService.CreateNewFile(file);
            if (newFileName == null)
                return Done<string>("Error while uploading file", ResponseCode.BadRequest);
            return Done(newFileName);
        }

        [HttpPost("PostMultiple")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string[]>>> PostMultiple()
        {
            var Ids = await fileService.CreateNewFiles(Request.Form.Files.ToArray());
            if (Ids?.Length > 0)
                return Done(Ids);
            return Done<string[]>("Error while uploading files", ResponseCode.BadRequest);
        }
    }
}