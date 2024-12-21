using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;
using System.Text;

namespace NewsManagementAPI.Controllers
{
    public class NewsMaterialsController : MyController
    {
        readonly NewsDbContext newsDbContext;
        public NewsMaterialsController(NewsDbContext newsDbContext, UserManager<NewsUser> userManager) : base(userManager)
        {
            this.newsDbContext = newsDbContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<NewsMaterialVM[]>>> Get(int pageNo, int pageSize)
        {
            try
            {
                var userRole = await userManager.Users.Where(x => x.Id == User.Identity!.Name).Select(x => x.Role).FirstOrDefaultAsync();
                NewsMaterialStatus? status = userRole == NewsUserRole.Reviewer ? NewsMaterialStatus.Reported
                    : userRole == NewsUserRole.Editor ? NewsMaterialStatus.Reviewed
                    : userRole == NewsUserRole.Designer ? NewsMaterialStatus.Edited
                    : userRole == NewsUserRole.Publisher ? NewsMaterialStatus.Designed : null;
                if (status == null)
                    return Done<NewsMaterialVM[]>("Unauthorized", ResponseCode.Unauthorized);
                var newsMaterials = await newsDbContext.NewsMaterials
                    .Where(x => x.CurrentStatus == status)
                    .SkipTake(pageNo, pageSize)
                    .Select(x => new NewsMaterialVM
                    {
                        ID = x.ID,
                        Content = x.Content,
                        Attachments = x.Attachments,
                        CurrentStatus = x.CurrentStatus,
                        EditorNote = x.EditorNote,
                        FinalAttachments = x.FinalAttachments,
                        FinalContent = x.FinalContent,
                        Images = x.Images,
                        Priority = x.Priority,
                        ReviewNote = x.ReviewNote,
                        Type = x.Type,
                        Urgency = x.Urgency,
                        PublishingStrategies = x.PublishingStrategies,
                        Histories = x.Histories!.Select(h => new NewsMaterialHistoryVM
                        {
                            EndDate = h.EndDate,
                            NewsUserID = h.NewsUserID,
                            ID = h.ID,
                            NewsMaterialID = h.NewsMaterialID,
                            StartDate = h.StartDate,
                            Status = h.Status,
                        }).ToList(),
                        CurrentUser = x.CurrentUser == null ? null : new() { DisplayName = x.CurrentUser.DisplayName, Id = x.CurrentUserID! }
                    })
                    .ToArrayAsync();
                return Done(newsMaterials);
            }
            catch (Exception ex)
            {
                return Done<NewsMaterialVM[]>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPost("ReportNews")]
        [Authorize(Policy = nameof(NewsUserRole.Reporter))]
        public async Task<ActionResult<ApiResponse<int>>> ReportNews([FromBody] ReportedNewsVM reportedNews)
        {
            try
            {
                NewsMaterial newsMaterial = new()
                {
                    Content = reportedNews.Content,
                    Attachments = reportedNews.Attachments,
                    Images = reportedNews.Images,
                    Priority = reportedNews.Priority,
                    Urgency = reportedNews.Urgency,
                    Type = reportedNews.Type,
                    CurrentUserID = User.Identity!.Name,
                    CurrentStatus = NewsMaterialStatus.Reported,
                    Histories = [new()
                    {
                        NewsUserID = User.Identity.Name!,
                        Status = NewsMaterialStatus.Reported,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow,
                    }]
                };
                await newsDbContext.NewsMaterials.AddAsync(newsMaterial);
                await newsDbContext.SaveChangesAsync();
                return Done(newsMaterial.ID);
            }
            catch (Exception ex)
            {
                return Done<int>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeingReview")]
        [Authorize(Policy = nameof(NewsUserRole.Reviewer))]
        public async Task<ActionResult<ApiResponse<bool>>> BeingReview(int newsMaterialId)
        {
            try
            {
                if (await newsDbContext.NewsMaterials.AnyAsync(x => x.ID == newsMaterialId && x.CurrentStatus == NewsMaterialStatus.Reported) == false)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, User.Identity!.Name)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Reviewing));

                await newsDbContext.NewsMaterialHistories.AddAsync(new()
                {
                    NewsUserID = User.Identity!.Name!,
                    NewsMaterialID = newsMaterialId,
                    StartDate = DateTime.UtcNow,
                    Status = NewsMaterialStatus.Reviewing,
                });
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndReview")]
        [Authorize(Policy = nameof(NewsUserRole.Reviewer))]
        public async Task<ActionResult<ApiResponse<bool>>> EndReview([FromBody] NewsMaterialReviewedVM reviewedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == reviewedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (newsMaterial.Content != reviewedVM.Content)
                    sb.AppendLine($"Content changed from: {newsMaterial.Content}, To: {reviewedVM.Content}");
                if (newsMaterial.Priority != reviewedVM.Priority)
                    sb.AppendLine($"Priority changed from: {newsMaterial.Priority}, To: {reviewedVM.Priority}");
                if (newsMaterial.Urgency != reviewedVM.Urgency)
                    sb.AppendLine($"Urgency changed from: {newsMaterial.Urgency}, To: {reviewedVM.Urgency}");
                newsMaterial.CurrentUserID = null;
                newsMaterial.CurrentStatus = NewsMaterialStatus.Reviewed;
                newsMaterial.ReviewNote = reviewedVM.ReviewNote;
                newsMaterial.Content = reviewedVM.Content;
                newsMaterial.Urgency = reviewedVM.Urgency;
                newsMaterial.Priority = reviewedVM.Priority;
                newsDbContext.NewsMaterials.Update(newsMaterial);

                await newsDbContext.NewsMaterialHistories
                    .Where(x => x.NewsMaterialID == newsMaterial.ID && x.NewsUserID == User.Identity!.Name
                        && x.Status == NewsMaterialStatus.Reviewing && x.EndDate == null)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, DateTime.UtcNow)
                                              .SetProperty(x => x.Status, NewsMaterialStatus.Reviewed));
                if (sb.Length > 0)
                    await CreateLog(sb.ToString(), newsMaterial.ID);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeginEdit")]
        [Authorize(Policy = nameof(NewsUserRole.Editor))]
        public async Task<ActionResult<ApiResponse<bool>>> BeginEdit(int newsMaterialId)
        {
            try
            {
                if (await newsDbContext.NewsMaterials.AnyAsync(x => x.ID == newsMaterialId && x.CurrentStatus == NewsMaterialStatus.Reviewed) == false)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, User.Identity!.Name)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Editing));

                await newsDbContext.NewsMaterialHistories.AddAsync(new()
                {
                    NewsUserID = User.Identity!.Name!,
                    NewsMaterialID = newsMaterialId,
                    StartDate = DateTime.UtcNow,
                    Status = NewsMaterialStatus.Editing,
                });
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndEdit")]
        [Authorize(Policy = nameof(NewsUserRole.Editor))]
        public async Task<ActionResult<ApiResponse<bool>>> EndEdit([FromBody] NewsMaterialEditedVM editedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == editedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (newsMaterial.FinalContent != editedVM.FinalContent)
                    sb.AppendLine($"Final content changed from: {newsMaterial.FinalContent}, To: {editedVM.FinalContent}");
                newsMaterial.CurrentUserID = null;
                newsMaterial.CurrentStatus = editedVM.Done ? NewsMaterialStatus.Edited : NewsMaterialStatus.Reviewed;
                newsMaterial.EditorNote = editedVM.EditorNote;
                newsMaterial.FinalContent = editedVM.FinalContent;
                newsMaterial.PublishingStrategies = editedVM.PublishingStrategies;
                newsDbContext.NewsMaterials.Update(newsMaterial);

                if (editedVM.Done)
                    await newsDbContext.NewsMaterialHistories
                        .Where(x => x.NewsMaterialID == newsMaterial.ID && x.NewsUserID == User.Identity!.Name
                            && x.Status == NewsMaterialStatus.Editing && x.EndDate == null)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, DateTime.UtcNow)
                                                  .SetProperty(x => x.Status, NewsMaterialStatus.Edited));
                else
                    await newsDbContext.NewsMaterialHistories
                        .Where(x => x.NewsMaterialID == newsMaterial.ID && x.NewsUserID == User.Identity!.Name
                            && x.Status == NewsMaterialStatus.Editing && x.EndDate == null)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, DateTime.UtcNow)
                                                  .SetProperty(x => x.Status, NewsMaterialStatus.Reviewed));
                if (sb.Length > 0)
                    await CreateLog(sb.ToString(), newsMaterial.ID);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeginDesign")]
        [Authorize(Policy = nameof(NewsUserRole.Editor))]
        public async Task<ActionResult<ApiResponse<bool>>> BeginDesign(int newsMaterialId)
        {
            try
            {
                if (await newsDbContext.NewsMaterials.AnyAsync(x => x.ID == newsMaterialId && x.CurrentStatus == NewsMaterialStatus.Edited) == false)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, User.Identity!.Name)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Designing));

                await newsDbContext.NewsMaterialHistories.AddAsync(new()
                {
                    NewsUserID = User.Identity!.Name!,
                    NewsMaterialID = newsMaterialId,
                    StartDate = DateTime.UtcNow,
                    Status = NewsMaterialStatus.Designing,
                });
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndDesign")]
        [Authorize(Policy = nameof(NewsUserRole.Editor))]
        public async Task<ActionResult<ApiResponse<bool>>> EndDesign([FromBody] NewsMaterialDesignedVM designedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == designedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (newsMaterial.FinalAttachments != designedVM.FinalAttachments)
                    sb.AppendLine($"Final attachments changed from: {newsMaterial.FinalAttachments}, To: {designedVM.FinalAttachments}");
                newsMaterial.CurrentUserID = null;
                newsMaterial.CurrentStatus = designedVM.Done ? NewsMaterialStatus.Designed : NewsMaterialStatus.Edited;
                newsMaterial.FinalAttachments = designedVM.FinalAttachments;
                newsDbContext.NewsMaterials.Update(newsMaterial);

                if (designedVM.Done)
                    await newsDbContext.NewsMaterialHistories
                        .Where(x => x.NewsMaterialID == newsMaterial.ID && x.NewsUserID == User.Identity!.Name
                            && x.Status == NewsMaterialStatus.Designing && x.EndDate == null)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, DateTime.UtcNow)
                                                  .SetProperty(x => x.Status, NewsMaterialStatus.Designed));
                else
                    await newsDbContext.NewsMaterialHistories
                        .Where(x => x.NewsMaterialID == newsMaterial.ID && x.NewsUserID == User.Identity!.Name
                            && x.Status == NewsMaterialStatus.Designing && x.EndDate == null)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, DateTime.UtcNow)
                                                  .SetProperty(x => x.Status, NewsMaterialStatus.Edited));
                if (sb.Length > 0)
                    await CreateLog(sb.ToString(), newsMaterial.ID);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("Publish")]
        [Authorize(Policy = nameof(NewsUserRole.Publisher))]
        public async Task<ActionResult<ApiResponse<bool>>> Publish([FromBody] NewsMaterialPublishedVM publishedVM)
        {
            try
            {
                if (await newsDbContext.NewsMaterials.AnyAsync(x => x.ID == publishedVM.ID && x.CurrentStatus == NewsMaterialStatus.Designed) == false)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                await newsDbContext.NewsMaterials.Where(x => x.ID == publishedVM.ID)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, x => null)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Published));

                await newsDbContext.NewsMaterialHistories.AddAsync(new()
                {
                    NewsUserID = User.Identity!.Name!,
                    NewsMaterialID = publishedVM.ID,
                    StartDate = DateTime.UtcNow,
                    Status = NewsMaterialStatus.Published,
                });
                await newsDbContext.NewsSocialMedias.AddRangeAsync(publishedVM.SocialMedias.Select(x => new NewsSocialMedia
                {
                    Url = x.Url,
                    NewsMaterialID = publishedVM.ID,
                    SocialMediaID = x.SocialMediaID,
                }));
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        async Task CreateLog(string log, int newsMaterialId)
        {
            await newsDbContext.NewsMaterialLogs.AddAsync(new()
            {
                Log = log,
                NewsUserID = User.Identity!.Name!,
                NewsMaterialID = newsMaterialId,
            });
        }
    }
}