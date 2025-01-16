using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.Services;
using NewsManagementAPI.ViewModels;
using NewsManagementAPI.ViewModels.NewsMaterialVMs;
using System;
using System.Linq;
using System.Text;

namespace NewsManagementAPI.Controllers
{
    public class NewsMaterialsController : MyController
    {
        readonly INotificationService notificationService;
        public NewsMaterialsController(NewsDbContext newsDbContext, UserManager<NewsUser> userManager, INotificationService notificationService) : base(userManager, newsDbContext)
        {
            this.notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PagingVM<NewsMaterialLiteVM>>>> Get(
            int pageNo = 1, int pageSize = 20,
            bool? requiresConfirmation = null,
            [ModelBinder(BinderType = typeof(CommaSeparatedEnumModelBinder<NewsMaterialStatus>))] List<NewsMaterialStatus>? statuses = null,//accepts statuses=1,4,3 etc...
                                                                                                                                            //string? statuses = null,//comma seperated
            DateTime? reportedAfter = null,
            DateTime? reportedBefore = null,
            DateTime? publishedAfter = null,
            DateTime? publishedBefore = null,
            Priority? priority = null,
            Urgency? urgency = null,
            ReporterType? reporterType = null,
            NewsRegion? newsRegion = null,
            bool? extremelyUrgent = null,
            string? userId = null)
        {
            try
            {
                var allowedStatuses = User.GetAllowedStatuses();
                if (statuses?.Except(allowedStatuses) is IEnumerable<NewsMaterialStatus> forbiddenStatuses && forbiddenStatuses.Any())
                    return Done<PagingVM<NewsMaterialLiteVM>>($"You don't have permission to view these statuses \"{string.Join(", ", forbiddenStatuses)}\"", ResponseCode.Unauthorized);
                statuses ??= allowedStatuses;
                var newsMaterialsSql = newsDbContext.NewsMaterials.AsQueryable();
                if (statuses != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => statuses.Contains(x.CurrentStatus));
                if (requiresConfirmation != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.ContentRequiresConfirmation == requiresConfirmation);
                if (reportedAfter != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.ReportedDate >= reportedAfter);
                if (reportedBefore != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.ReportedDate <= reportedBefore);
                if (publishedAfter != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.PublishDate >= publishedAfter);
                if (publishedBefore != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.PublishDate <= publishedBefore);
                if (priority != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.Priority == priority);
                if (urgency != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.Urgency == urgency);
                if (extremelyUrgent != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.ExtremelyUrgent == extremelyUrgent);
                if (reporterType != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.CurrentUser!.ReporterType == reporterType);
                if (newsRegion != null)
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.NewsRegion == newsRegion);
                if (!User.IsManager())
                {
                    if (User.HasRole(NewsUserRole.Editor))
                        newsMaterialsSql = newsMaterialsSql.Where(x => x.CurrentUserID == User.Identity!.Name || x.CurrentUserID == null);
                    else
                        newsMaterialsSql = newsMaterialsSql.Where(x => x.CurrentUserID == User.Identity!.Name);
                }
                else if (!string.IsNullOrEmpty(userId))
                    newsMaterialsSql = newsMaterialsSql.Where(x => x.CurrentUserID == userId);

                var newsMaterials = await newsMaterialsSql.ToNewsMaterialLiteVM()
                    .OrderByDescending(x => x.ExtremelyUrgent).ThenByDescending(x => x.Urgency).ThenByDescending(x => x.Priority)
                    .SkipTake(pageNo, pageSize)
                    .ToArrayAsync();

                return Done(new PagingVM<NewsMaterialLiteVM> { Count = await newsMaterialsSql.CountAsync(), Items = newsMaterials });
            }
            catch (Exception ex)
            {
                return Done<PagingVM<NewsMaterialLiteVM>>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<NewsMaterialVM>>> Get(int id)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == id).ToNewsMaterialVM().FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<NewsMaterialVM>("News material not found!", ResponseCode.NotFound);
                if (!User.IsManager() && newsMaterial.CurrentUser?.Id != User.Identity!.Name)
                    return Done<NewsMaterialVM>("This news material is not assigned to you!", ResponseCode.Unauthorized);
                return Done(newsMaterial);
            }
            catch (Exception ex)
            {
                return Done<NewsMaterialVM>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpGet("Details/{id}")]
        [Authorize(Policy = UserRoleNames.Admin)]
        public async Task<ActionResult<ApiResponse<NewsMaterialDetailsVM>>> GetDetails(int id)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == id).ToNewsMaterialDetailsVM().FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<NewsMaterialDetailsVM>("News material not found!", ResponseCode.NotFound);
                if (!User.IsManager() && newsMaterial.CurrentUser?.Id != User.Identity!.Name)
                    return Done<NewsMaterialDetailsVM>("This news material is not assigned to you!", ResponseCode.Unauthorized);
                return Done(newsMaterial);
            }
            catch (Exception ex)
            {
                return Done<NewsMaterialDetailsVM>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPost("ReportNews")]
        [Authorize(Policy = UserRoleNames.Reporter)]
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
                    ExtremelyUrgent = reportedNews.ExtremelyUrgent,
                    NewsRegion = reportedNews.NewsRegion,
                    CurrentUserID = User.Identity!.Name,
                    CurrentStatus = NewsMaterialStatus.Reported,
                    ReportedDate = DateTime.UtcNow,
                    Histories = [new()
                    {
                        NewsUserID = User.Identity.Name!,
                        Status = NewsMaterialStatus.Reported,
                        Date = DateTime.UtcNow,
                    }]
                };
                if (newsMaterial.ExtremelyUrgent)
                {
                    var reviewerIds = await newsDbContext.UserClaims
                        .Where(x => x.ClaimValue == NewsUserRole.Reviewer.GetPolicyNumber())
                        .Select(x => x.UserId)
                        .ToArrayAsync();
                    foreach (var reviewerId in reviewerIds)
                    {
                        await notificationService.AddNotification(new()
                        {
                            UserId = reviewerId,
                            Title = "خبر فوري جديد",
                            Text = $"تم رصد خبر فوري جديد.",
                            Topic = "ExtremelyUrgent",
                        });
                    }
                }
                await newsDbContext.NewsMaterials.AddAsync(newsMaterial);
                await newsDbContext.SaveChangesAsync();
                return Done(newsMaterial.ID);
            }
            catch (Exception ex)
            {
                return Done<int>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndReview")]
        [Authorize(Policy = UserRoleNames.Reviewer)]
        public async Task<ActionResult<ApiResponse<bool>>> EndReview([FromBody] NewsMaterialReviewedVM reviewedVM)
        {
            try
            {
                if (reviewedVM.PublishingStrategies.Count == 0)
                    return Done<bool>("Please select a publishing strategy(s)!", ResponseCode.BadRequest);
                var newsMaterial = await newsDbContext.NewsMaterials.Include(x => x.PublishingStrategies).AsNoTracking().FirstOrDefaultAsync(x => x.ID == reviewedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (newsMaterial.CurrentStatus >= NewsMaterialStatus.Reviewed)
                    return Done<bool>("تم تدقبق الخبر مسبقا", ResponseCode.BadRequest);
                if (string.IsNullOrWhiteSpace(reviewedVM.Images))
                    reviewedVM.Images = null;
                if (reviewedVM.Images?.Split(',').Except(newsMaterial.Images?.Split(',') ?? []).Any() == true)
                    return Done<bool>("لا يمكنك إضافة الصور", ResponseCode.BadRequest);
                if (string.IsNullOrWhiteSpace(reviewedVM.Attachments))
                    reviewedVM.Attachments = null;
                if (reviewedVM.Attachments?.Split(',').Except(newsMaterial.Attachments?.Split(',') ?? []).Any() == true)
                    return Done<bool>("لا يمكنك إضافة المرفقات", ResponseCode.BadRequest);
                StringBuilder sb = new();
                if (newsMaterial.Content != reviewedVM.Content)
                    sb.AppendLine($"تم تعديل المحتوى من: \"{newsMaterial.Content}\" إلى: \"{reviewedVM.Content}\"");
                if (newsMaterial.Priority != reviewedVM.Priority)
                    sb.AppendLine($"تم تعديل الأهمية من: \"{newsMaterial.Priority}\", إلى: \"{reviewedVM.Priority}\"");
                if (newsMaterial.Urgency != reviewedVM.Urgency)
                    sb.AppendLine($"تم تعديل العجلة من: \"{newsMaterial.Urgency}\", إلى: \"{reviewedVM.Urgency}\"");
                if (newsMaterial.ExtremelyUrgent != reviewedVM.ExtremelyUrgent)
                    sb.AppendLine($"تم تعديل مستعجل جدا من: \"{newsMaterial.ExtremelyUrgent}\", إلى: \"{reviewedVM.ExtremelyUrgent}\"");
                if (newsMaterial.ContentRequiresConfirmation != reviewedVM.ContentRequiresConfirmation)
                    sb.AppendLine($"تم تعديل المحتوى يحتاج الموافقة من: \"{newsMaterial.ContentRequiresConfirmation}\", إلى: \"{reviewedVM.ContentRequiresConfirmation}\"");
                if (newsMaterial.Images != reviewedVM.Images)
                    sb.AppendLine($"تم تعديل الصور من: \"{newsMaterial.Images}\", إلى: \"{reviewedVM.Images}\"");
                if (newsMaterial.Attachments != reviewedVM.Attachments)
                    sb.AppendLine($"تم تعديل المرفقات من: \"{newsMaterial.Attachments}\", إلى: \"{reviewedVM.Attachments}\"");
                if (newsMaterial.ContentRequiresConfirmation)
                    newsMaterial.CurrentStatus = NewsMaterialStatus.WaitingPublisherConfirmation;
                else if (reviewedVM.ExtremelyUrgent)
                {
                    if (reviewedVM.SkipProofreading)
                        newsMaterial.CurrentStatus = NewsMaterialStatus.Designed;//go to publishing
                    else
                        newsMaterial.CurrentStatus = NewsMaterialStatus.Edited;//skip editing
                }
                else
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Reviewed;
                newsMaterial.CurrentUserID = null;
                newsMaterial.ReviewNote = reviewedVM.ReviewNote;
                newsMaterial.Content = reviewedVM.Content;
                newsMaterial.Urgency = reviewedVM.Urgency;
                newsMaterial.Priority = reviewedVM.Priority;
                newsMaterial.DeadLine = reviewedVM.DeadLine;
                newsMaterial.ExtremelyUrgent = reviewedVM.ExtremelyUrgent;
                newsMaterial.ContentRequiresConfirmation = reviewedVM.ContentRequiresConfirmation;
                newsMaterial.Images = reviewedVM.Images;
                newsMaterial.Attachments = reviewedVM.Attachments;
                foreach (var strat in reviewedVM.PublishingStrategies)
                {
                    if (newsMaterial.PublishingStrategies.Any(x => x.PublishingStrategy == strat) == false)
                        newsMaterial.PublishingStrategies.Add(new() { NewsMaterialID = newsMaterial.ID, PublishingStrategy = strat });
                }
                var deleteStratIds = newsMaterial.PublishingStrategies.ExceptBy(reviewedVM.PublishingStrategies, x => x.PublishingStrategy).Select(x => x.ID).ToArray();
                newsMaterial.PublishingStrategies.RemoveAll(x => deleteStratIds.Contains(x.ID));
                await newsDbContext.NewsMaterialPublishingStrategies.Where(x => deleteStratIds.Contains(x.ID)).ExecuteDeleteAsync();
                newsDbContext.NewsMaterials.Update(newsMaterial);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, newsMaterial.CurrentStatus, User.Identity!.Name!, null);

                if (sb.Length > 0)
                    await newsDbContext.CreateNewMaterialLog(newsMaterial.ID, User.Identity!.Name!, sb.ToString());
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("Assign")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Assign(int newsMaterialId, string asigneeId)
        {
            try
            {
                if (User.IsManager() || (User.HasRole(NewsUserRole.Editor) && asigneeId == User.Identity!.Name))
                {
                    var asigneeClaims = await newsDbContext.UserClaims.Where(x => x.UserId == asigneeId).AsNoTracking().ToArrayAsync();
                    NewsMaterialStatus status;
                    if (User.HasRole(NewsUserRole.EditingManager))
                    {
                        if (!asigneeClaims.HasRole(NewsUserRole.Editor))
                            return Done<bool>("You can only assign to editors!", ResponseCode.BadRequest);
                        status = NewsMaterialStatus.AssignedForEditing;
                    }
                    else if (User.HasRole(NewsUserRole.ProofreadingManager))
                    {
                        if (!asigneeClaims.HasRole(NewsUserRole.Proofreader))
                            return Done<bool>("You can only assign to proofreaders!", ResponseCode.BadRequest);
                        status = NewsMaterialStatus.AssignedForProofreading;
                    }
                    else if (User.HasRole(NewsUserRole.DesigningManager))
                    {
                        if (!asigneeClaims.HasRole(NewsUserRole.Designer))
                            return Done<bool>("You can only assign to designers!", ResponseCode.BadRequest);
                        status = NewsMaterialStatus.AssignedForDesigning;
                    }
                    else if (User.HasRole(NewsUserRole.PublishingManager))
                    {
                        if (!asigneeClaims.HasRole(NewsUserRole.Publisher))
                            return Done<bool>("You can only assign to publishers!", ResponseCode.BadRequest);
                        status = NewsMaterialStatus.AssignedForPublishing;
                    }
                    else if (User.HasRole(NewsUserRole.Editor) && asigneeId == User.Identity!.Name)
                    {
                        status = NewsMaterialStatus.AssignedForEditing;
                    }
                    //else if (User.HasRole(NewsUserRole.Admin))
                    //    status = NewsMaterialStatus.AssignedForDesigning;
                    else
                        return Done<bool>("Unauthorized", ResponseCode.Unauthorized);

                    var oldUserId = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                        .Select(x => x.CurrentUserID).FirstOrDefaultAsync();
                    if (oldUserId != null && asigneeId != oldUserId)
                    {
                        var oldUserName = await userManager.Users.Where(x => x.Id == oldUserId)
                            .Select(x => x.DisplayName).FirstOrDefaultAsync();
                        var newUserName = await userManager.Users.Where(x => x.Id == asigneeId)
                            .Select(x => x.DisplayName).FirstOrDefaultAsync();
                        await newsDbContext.CreateNewMaterialLog(newsMaterialId, User.Identity!.Name!, $"تم تغيير المستخدم من {oldUserName} إلى {newUserName}");
                    }
                    var count = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, asigneeId)
                                                  .SetProperty(x => x.CurrentStatus, status));
                    if (count == 0)
                        return Done<bool>("Not found!", ResponseCode.NotFound);

                    await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, status, asigneeId, User.Identity!.Name!);
                    await newsDbContext.SaveChangesAsync();
                    return Done(true);
                }
                else
                    return Done<bool>("You are not authorized to do this action!", ResponseCode.Unauthorized);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("Begin")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Begin(int newsMaterialId)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials
                    .Where(x => x.ID == newsMaterialId)
                    .Select(x => new { x.CurrentStatus, x.CurrentUserID, x.ContentRequiresConfirmation, x.ContentConfirmed })
                    .FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (newsMaterial.CurrentUserID != User.Identity!.Name)
                    return Done<bool>("This news material is not assigned to you!", ResponseCode.Unauthorized);
                if (newsMaterial.ContentRequiresConfirmation && !newsMaterial.ContentConfirmed)
                    return Done<bool>("This news material requires the publisher's confirmation first!", ResponseCode.BadRequest);
                NewsMaterialStatus status;
                if (User.HasRole(NewsUserRole.Editor))
                {
                    if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForEditing)
                        return Done<bool>("This news material is not in the editing stage", ResponseCode.BadRequest);
                    status = NewsMaterialStatus.Editing;
                }
                else if (User.HasRole(NewsUserRole.Proofreader))
                {
                    if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForProofreading)
                        return Done<bool>("This news material is not in the proofreading stage", ResponseCode.BadRequest);
                    status = NewsMaterialStatus.Proofreading;
                }
                else if (User.HasRole(NewsUserRole.Designer))
                {
                    if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForDesigning)
                        return Done<bool>("This news material is not in the designing stage", ResponseCode.BadRequest);
                    status = NewsMaterialStatus.Designing;
                }
                //else if (User.HasRole(NewsUserRole.Publisher))
                //{
                //    if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForPublishing)
                //        return Done<bool>("This news material is not in the publishing stage", ResponseCode.BadRequest);
                //    status = NewsMaterialStatus.Publishing;
                //}
                else
                    return Done<bool>("You don't need to begin!", ResponseCode.BadRequest);

                await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentStatus, status));
                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, status, User.Identity!.Name!, null);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndEdit")]
        [Authorize(Policy = UserRoleNames.Editor)]
        public async Task<ActionResult<ApiResponse<bool>>> EndEdit([FromBody] NewsMaterialEditedVM editedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == editedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (!string.IsNullOrEmpty(newsMaterial.FinalContent) && newsMaterial.FinalContent != editedVM.FinalContent)
                    sb.AppendLine($"تم تعديل المحتوى النهائي من: {newsMaterial.FinalContent}, إلى: {editedVM.FinalContent}");
                if (!string.IsNullOrEmpty(newsMaterial.EditorNote) && newsMaterial.EditorNote != editedVM.EditorNote)
                    sb.AppendLine($"تم تعديل ملاحظة المحرر من: {newsMaterial.EditorNote}, إلى: {editedVM.EditorNote}");
                if (editedVM.SaveOnly)
                {

                }
                else if (editedVM.SkipProofreading)
                {
                    if (await newsDbContext.UserClaims.AnyAsync(x => x.UserId == User.Identity!.Name && x.ClaimValue == NewsUserRole.Proofreader.GetPolicyNumber()) == false)
                        return Done<bool>("You don't have proofreading permission!", ResponseCode.Unauthorized);
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Proofreaded;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Proofreaded, User.Identity!.Name!, null);
                }
                else
                {
                    newsMaterial.CurrentUserID = null;
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Edited;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Edited, User.Identity!.Name!, null);
                }
                newsMaterial.EditorNote = editedVM.EditorNote;
                newsMaterial.FinalContent = editedVM.FinalContent;
                newsMaterial.SendToDesigner = editedVM.SendToDesigner;
                if (newsMaterial.PublishingStrategies.Count == 1)
                    newsMaterial.FinalPublishingStrategy = newsMaterial.PublishingStrategies[0].PublishingStrategy;
                else
                {
                    if (editedVM.PublishingStrategy == null)
                        return Done<bool>("Please select a publishing strategy!", ResponseCode.BadRequest);
                    newsMaterial.FinalPublishingStrategy = editedVM.PublishingStrategy.Value;
                }
                newsDbContext.NewsMaterials.Update(newsMaterial);

                if (sb.Length > 0)
                    await newsDbContext.CreateNewMaterialLog(newsMaterial.ID, User.Identity!.Name!, sb.ToString());
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndProofread")]
        [Authorize(Policy = UserRoleNames.Proofreader)]
        public async Task<ActionResult<ApiResponse<bool>>> EndProofread([FromBody] NewsMaterialProofreadedVM proofreadedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == proofreadedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (newsMaterial.FinalContent != proofreadedVM.FinalContent)
                    sb.AppendLine($"تم تعديل المحتوى النهائي من: {newsMaterial.FinalContent}, إلى: {proofreadedVM.FinalContent}");

                newsMaterial.CurrentUserID = null;
                if (newsMaterial.ExtremelyUrgent || !newsMaterial.SendToDesigner)
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Designed;
                else
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Proofreaded;
                newsMaterial.FinalContent = proofreadedVM.FinalContent;
                newsDbContext.NewsMaterials.Update(newsMaterial);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Proofreaded, User.Identity!.Name!, null);
                if (sb.Length > 0)
                    await newsDbContext.CreateNewMaterialLog(newsMaterial.ID, User.Identity!.Name!, sb.ToString());
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("EndDesign")]
        [Authorize(Policy = UserRoleNames.Designer)]
        public async Task<ActionResult<ApiResponse<bool>>> EndDesign([FromBody] NewsMaterialDesignedVM designedVM)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.AsNoTracking().FirstOrDefaultAsync(x => x.ID == designedVM.ID);
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                StringBuilder sb = new();
                if (!string.IsNullOrEmpty(newsMaterial.FinalAttachments) && newsMaterial.FinalAttachments != designedVM.FinalAttachments)
                    sb.AppendLine($"تم تعديل المرفقات النهائية من: {newsMaterial.FinalAttachments}, To: {designedVM.FinalAttachments}");
                if (!designedVM.SaveOnly)
                {
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Designed;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Designed, User.Identity!.Name!, null);
                    newsMaterial.CurrentUserID = null;
                }
                newsMaterial.FinalAttachments = designedVM.FinalAttachments;
                newsDbContext.NewsMaterials.Update(newsMaterial);

                if (sb.Length > 0)
                    await newsDbContext.CreateNewMaterialLog(newsMaterial.ID, User.Identity!.Name!, sb.ToString());
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("Publish")]
        [Authorize(Policy = UserRoleNames.Publisher)]
        public async Task<ActionResult<ApiResponse<bool>>> Publish([FromBody] NewsMaterialPublishedVM publishedVM)
        {
            try
            {
                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == publishedVM.ID)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, x => null)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Published)
                                              .SetProperty(x => x.PublishDate, DateTime.UtcNow));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                await newsDbContext.CreateNewsMaterialHistory(publishedVM.ID, NewsMaterialStatus.Published, User.Identity!.Name!, null);
                await newsDbContext.NewsSocialMedias.AddRangeAsync(publishedVM.SocialMedias.Select(x => new NewsSocialMedia
                {
                    NewsMaterialID = publishedVM.ID,
                    Url = x.Url,
                    SocialMediaID = x.SocialMediaID,
                    Schedule = x.Schedule,
                }));
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("ConfirmContent")]
        [Authorize(Policy = UserRoleNames.Publisher)]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmContent(int materialId, bool confirmed, string? note = null)
        {
            try
            {
                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == materialId && x.ContentRequiresConfirmation)
                                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.ContentConfirmed, confirmed)
                                                                  .SetProperty(x => x.PublisherNote, note));
                if (count == 0)
                    return Done<bool>("Not found", ResponseCode.NotFound);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("SendBack")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> SendBack(int materialId, string note)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == materialId)
                    .Select(x => new
                    {
                        x.CurrentStatus,
                        x.CurrentUserID
                    }).FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                NewsMaterialStatus status;
                switch (newsMaterial.CurrentStatus)
                {
                    case NewsMaterialStatus.Reviewed:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.EditingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Reported;
                        break;
                    case NewsMaterialStatus.AssignedForEditing:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Editor) && !User.HasRole(NewsUserRole.EditingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Reviewed;
                        break;
                    case NewsMaterialStatus.Editing:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Editor) && !User.HasRole(NewsUserRole.EditingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Reviewed;
                        break;
                    case NewsMaterialStatus.Edited:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.ProofreadingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Reviewed;
                        break;
                    case NewsMaterialStatus.AssignedForProofreading:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Proofreader) && !User.HasRole(NewsUserRole.ProofreadingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Edited;
                        break;
                    case NewsMaterialStatus.Proofreading:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Proofreader) && !User.HasRole(NewsUserRole.ProofreadingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Edited;
                        break;
                    case NewsMaterialStatus.Proofreaded:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.DesigningManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Edited;
                        break;
                    case NewsMaterialStatus.AssignedForDesigning:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Designer) && !User.HasRole(NewsUserRole.DesigningManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Proofreaded;
                        break;
                    case NewsMaterialStatus.Designing:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Designer) && !User.HasRole(NewsUserRole.DesigningManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Proofreaded;
                        break;
                    case NewsMaterialStatus.Designed:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.PublishingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Proofreaded;
                        break;
                    case NewsMaterialStatus.AssignedForPublishing:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Publisher) && !User.HasRole(NewsUserRole.PublishingManager))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Designed;
                        break;
                    case NewsMaterialStatus.WaitingPublisherConfirmation:
                        if (!User.HasRole(NewsUserRole.Admin) && !User.HasRole(NewsUserRole.Reviewer))
                            return Done<bool>("Unauthorized", ResponseCode.Unauthorized);
                        status = NewsMaterialStatus.Reported;
                        break;
                    case NewsMaterialStatus.Draft:
                        return Done<bool>("This report is still a draft", ResponseCode.BadRequest);
                    case NewsMaterialStatus.Reported:
                        return Done<bool>("This report is still in its first stage", ResponseCode.BadRequest);
                    case NewsMaterialStatus.Published:
                        return Done<bool>("This report has been published", ResponseCode.BadRequest);
                    default:
                        return Done<bool>("Bad request!", ResponseCode.BadRequest);
                }
                await newsDbContext.NewsMaterials.Where(x => x.ID == materialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentStatus, status)
                                              .SetProperty(x => x.CurrentUserID, x => null));
                await newsDbContext.CreateNewsMaterialHistory(materialId, status, User.Identity!.Name!, null);
                await newsDbContext.CreateNewMaterialLog(materialId, User.Identity.Name!, note);
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }
    }
}