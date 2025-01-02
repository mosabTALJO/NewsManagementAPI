using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using NewsManagementAPI.ViewModels.NewsMaterialVMs;

namespace NewsManagementAPI.Controllers
{
    public class SocialMediaController : MyController
    {
        public SocialMediaController(UserManager<NewsUser> userManager, NewsDbContext newsDbContext) : base(userManager, newsDbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<SocialMediaVM[]>>> Get()
        {
            try
            {
                var socailMediaVMs = await newsDbContext.SocialMedias.Select(x => new SocialMediaVM
                {
                    ID = x.ID,
                    Url = x.Url,
                    Name = x.Name,
                }).ToArrayAsync();
                return Done(socailMediaVMs);
            }
            catch (Exception ex)
            {
                return Done<SocialMediaVM[]>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> Post([FromBody] SocialMediaVM socialMediaVM)
        {
            try
            {
                SocialMedia socialMedia = new()
                {
                    Name = socialMediaVM.Name,
                    Url = socialMediaVM.Url,
                };
                await newsDbContext.SocialMedias.AddAsync(socialMedia);
                await newsDbContext.SaveChangesAsync();
                return Done(socialMedia.ID);
            }
            catch (Exception ex)
            {
                return Done<int>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<bool>>> Put([FromBody] SocialMediaVM socialMediaVM)
        {
            try
            {
                var count = await newsDbContext.SocialMedias.Where(x => x.ID == socialMediaVM.ID)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.Name, socialMediaVM.Name)
                                              .SetProperty(x => x.Url, socialMediaVM.Url));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int socialMediaId)
        {
            try
            {
                var count = await newsDbContext.SocialMedias.Where(x => x.ID == socialMediaId).ExecuteDeleteAsync();
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }
    }
}