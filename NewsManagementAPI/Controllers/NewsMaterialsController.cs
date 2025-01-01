using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Data;
using NewsManagementAPI.Extentions;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.Services;
using NewsManagementAPI.ViewModels.NewsMaterialVMs;
using System;
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
        public async Task<ActionResult<ApiResponse<NewsMaterialVM[]>>> Get(
            int pageNo = 1, int pageSize = 20,
            bool? requiresConfirmation = null,
            [ModelBinder(BinderType = typeof(CommaSeparatedEnumModelBinder<NewsMaterialStatus>))] List<NewsMaterialStatus>? statuses = null,//accepts statuses=1,4,3 etc...
                                                                                                                                            //string? statuses = null,//comma seperated
            DateTime? reportDate = null,
            string? userId = null)
        {
            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (!User.HasPemission(status))
                        return Done<NewsMaterialVM[]>($"You don't have permission to view {status} reports!", ResponseCode.Unauthorized);
                }
            }
            else if (string.IsNullOrEmpty(userId) && !User.HasRole(NewsUserRole.Admin))
                return Done<NewsMaterialVM[]>("You don't have permission to view all news materials!", ResponseCode.Unauthorized);

            var newsMaterialsSql = newsDbContext.NewsMaterials.AsQueryable();
            if (statuses != null && requiresConfirmation != null)
                newsMaterialsSql = newsMaterialsSql.Where(x => statuses.Contains(x.CurrentStatus) || x.ContentRequiresConfirmation == requiresConfirmation);
            else if (statuses != null)
                newsMaterialsSql = newsMaterialsSql.Where(x => statuses.Contains(x.CurrentStatus));
            else if (requiresConfirmation != null)
                newsMaterialsSql = newsMaterialsSql.Where(x => x.ContentRequiresConfirmation == requiresConfirmation);
            if (reportDate != null)
                newsMaterialsSql = newsMaterialsSql.Where(x => x.ReportedDate == reportDate);
            if (!string.IsNullOrEmpty(userId))
                newsMaterialsSql = newsMaterialsSql.Where(x => x.CurrentUserID == userId);

            var newsMaterials = await newsMaterialsSql.ToNewsMaterialVM()
                .OrderByDescending(x => x.Urgency).ThenByDescending(x => x.Priority)
                .SkipTake(pageNo, pageSize)
                .ToArrayAsync();

            return Done(newsMaterials);
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
                    CurrentUserID = User.Identity!.Name,
                    CurrentStatus = NewsMaterialStatus.Reported,
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
                            Title = "Extremely Urgent",
                            Text = $"A new extremely urgent news material has been reported.",
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

        //[HttpPut("BeingReview")]
        //[Authorize(Policy = UserRoleNames.Reviewer)]
        //public async Task<ActionResult<ApiResponse<bool>>> BeingReview(int newsMaterialId)
        //{
        //    try
        //    {
        //        if (await newsDbContext.NewsMaterials.AnyAsync(x => x.ID == newsMaterialId && x.CurrentStatus == NewsMaterialStatus.Reported) == false)
        //            return Done<bool>("Not found!", ResponseCode.NotFound);
        //        var newsMaterial = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
        //            .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, User.Identity!.Name)
        //                                      .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Reviewing));

        //        await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.Reviewing, User.Identity!.Name!, null);
        //        await newsDbContext.SaveChangesAsync();
        //        return Done(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
        //    }
        //}

        [HttpPut("EndReview")]
        [Authorize(Policy = UserRoleNames.Reviewer)]
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
                if (newsMaterial.ExtremelyUrgent != reviewedVM.ExtremelyUrgent)
                    sb.AppendLine($"\"Extremely Urgent\" changed from: {newsMaterial.ExtremelyUrgent}, To: {reviewedVM.ExtremelyUrgent}");
                if (newsMaterial.ContentRequiresConfirmation != reviewedVM.ContentRequiresConfirmation)
                    sb.AppendLine($"\"Content Requires Confirmation\" changed from: {newsMaterial.ContentRequiresConfirmation}, To: {reviewedVM.ContentRequiresConfirmation}");
                if (reviewedVM.ExtremelyUrgent)
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Edited;//skip editing
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
                newsMaterial.PublishingStrategies = reviewedVM.PublishingStrategies.Select(x => new NewsMaterialPublishStrat { NewsMaterialID = newsMaterial.ID, PublishingStrategy = x }).ToList();
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

        [HttpPut("AssignEdit")]
        [Authorize(Policy = UserRoleNames.EditingManager)]
        public async Task<ActionResult<ApiResponse<bool>>> AssignEdit(int newsMaterialId, string editorId)
        {
            try
            {
                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, editorId)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.AssignedForEditing));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.AssignedForEditing, editorId, User.Identity!.Name!);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeginEdit")]
        [Authorize(Policy = UserRoleNames.Editor)]
        public async Task<ActionResult<ApiResponse<bool>>> BeginEdit(int newsMaterialId)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials
                    .Where(x => x.ID == newsMaterialId)
                    .Select(x => new { x.CurrentStatus, x.CurrentUserID, x.ContentRequiresConfirmation, x.ContentConfirmed })
                    .FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForEditing)
                    return Done<bool>("This news material is not in the editing stage!", ResponseCode.BadRequest);
                if (newsMaterial.CurrentUserID != User.Identity!.Name)
                    return Done<bool>("This news material is not assigned to you!", ResponseCode.Unauthorized);
                if (newsMaterial.ContentRequiresConfirmation && !newsMaterial.ContentConfirmed)
                    return Done<bool>("This news material requires the publisher's confirmation first!", ResponseCode.BadRequest);

                await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Editing));
                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.Editing, User.Identity!.Name!, null);
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
                    sb.AppendLine($"Final content changed from: {newsMaterial.FinalContent}, To: {editedVM.FinalContent}");
                if (!editedVM.Done)
                {
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Reviewed;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Edited, User.Identity!.Name!, null);
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
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Edited;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Edited, User.Identity!.Name!, null);
                }
                newsMaterial.CurrentUserID = null;
                newsMaterial.EditorNote = editedVM.EditorNote;
                newsMaterial.FinalContent = editedVM.FinalContent;
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

        [HttpPut("AssignProofread")]
        [Authorize(Policy = UserRoleNames.ProofreadingManager)]
        public async Task<ActionResult<ApiResponse<bool>>> AssignProofread(int newsMaterialId, string proofreaderId)
        {
            try
            {
                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, proofreaderId)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.AssignedForProofreading));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.AssignedForProofreading, proofreaderId, User.Identity!.Name!);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeginProofread")]
        [Authorize(Policy = UserRoleNames.Proofreader)]
        public async Task<ActionResult<ApiResponse<bool>>> BeginProofread(int newsMaterialId)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials
                    .Where(x => x.ID == newsMaterialId)
                    .Select(x => new { x.CurrentStatus, x.CurrentUserID })
                    .FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForProofreading)
                    return Done<bool>("This news material is not in the proofreading stage!", ResponseCode.BadRequest);
                if (newsMaterial.CurrentUserID != User.Identity!.Name)
                    return Done<bool>("This news material is not assigned to you!", ResponseCode.Unauthorized);

                await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Proofreading));
                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.Proofreading, User.Identity!.Name!, null);
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
                    sb.AppendLine($"Final content changed from: {newsMaterial.FinalContent}, To: {proofreadedVM.FinalContent}");

                newsMaterial.CurrentUserID = null;
                if (newsMaterial.ExtremelyUrgent)
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

        [HttpPut("AssignDesign")]
        [Authorize(Policy = UserRoleNames.DesigningManager)]
        public async Task<ActionResult<ApiResponse<bool>>> AssignDesign(int newsMaterialId, string designerId)
        {
            try
            {
                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, designerId)
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.AssignedForDesigning));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.AssignedForEditing, designerId, User.Identity!.Name!);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
            }
            catch (Exception ex)
            {
                return Done<bool>(ex.AllMessages(), ResponseCode.BadRequest);
            }
        }

        [HttpPut("BeginDesign")]
        [Authorize(Policy = UserRoleNames.Designer)]
        public async Task<ActionResult<ApiResponse<bool>>> BeginDesign(int newsMaterialId)
        {
            try
            {
                var newsMaterial = await newsDbContext.NewsMaterials
                    .Where(x => x.ID == newsMaterialId)
                    .Select(x => new { x.CurrentStatus, x.CurrentUserID, x.ContentRequiresConfirmation, x.ContentConfirmed })
                    .FirstOrDefaultAsync();
                if (newsMaterial == null)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                if (newsMaterial.CurrentStatus != NewsMaterialStatus.AssignedForDesigning)
                    return Done<bool>("This news material is not in the designing stage!", ResponseCode.BadRequest);
                if (newsMaterial.CurrentUserID != User.Identity!.Name)
                    return Done<bool>("This news material is not assigned to you!", ResponseCode.Unauthorized);
                if (newsMaterial.ContentRequiresConfirmation && !newsMaterial.ContentConfirmed)
                    return Done<bool>("This news material requires the publisher's confirmation first!", ResponseCode.BadRequest);

                await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Designing));
                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, NewsMaterialStatus.Designing, User.Identity!.Name!, null);
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
                    sb.AppendLine($"Final attachments changed from: {newsMaterial.FinalAttachments}, To: {designedVM.FinalAttachments}");
                if (designedVM.Done)
                {
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Designed;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Designed, User.Identity!.Name!, null);
                }
                else
                {
                    newsMaterial.CurrentStatus = NewsMaterialStatus.Proofreaded;
                    await newsDbContext.CreateNewsMaterialHistory(newsMaterial.ID, NewsMaterialStatus.Designed, User.Identity!.Name!, null);
                }
                newsMaterial.CurrentUserID = null;
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
                                              .SetProperty(x => x.CurrentStatus, NewsMaterialStatus.Published));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);
                await newsDbContext.CreateNewsMaterialHistory(publishedVM.ID, NewsMaterialStatus.Published, User.Identity!.Name!, null);
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

        [HttpPut("Assign")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Assign(int newsMaterialId, string asigneeId)
        {
            try
            {
                if (!User.IsManager())
                    return Done<bool>("You are not authorized to do this action!", ResponseCode.Unauthorized);
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
                //else if (User.HasRole(NewsUserRole.Admin))
                //    status = NewsMaterialStatus.AssignedForDesigning;
                else
                    return Done<bool>("Unauthorized", ResponseCode.Unauthorized);

                var count = await newsDbContext.NewsMaterials.Where(x => x.ID == newsMaterialId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.CurrentUserID, asigneeId)
                                              .SetProperty(x => x.CurrentStatus, status));
                if (count == 0)
                    return Done<bool>("Not found!", ResponseCode.NotFound);

                await newsDbContext.CreateNewsMaterialHistory(newsMaterialId, status, asigneeId, User.Identity!.Name!);
                await newsDbContext.SaveChangesAsync();
                return Done(true);
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
    }
}