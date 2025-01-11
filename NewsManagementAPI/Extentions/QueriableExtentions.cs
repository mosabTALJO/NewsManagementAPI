using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NewsManagementAPI.Data;
using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels.NewsMaterialVMs;
using NewsManagementAPI.ViewModels.UserVMs;
using System.Security.Claims;

namespace NewsManagementAPI.Extentions
{
    public static class QueriableExtentions
    {
        public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, int pageNo, int pageSize)
            => source.Skip((pageNo - 1) * pageSize).Take(pageSize);
        public static IQueryable<NewsMaterialVM> ToNewsMaterialVM(this IQueryable<NewsMaterial> source)
        {
            return source.Select(x => new NewsMaterialVM
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
                NewsRegion = x.NewsRegion,
                ReportedDate = x.ReportedDate,
                Type = x.Type,
                Urgency = x.Urgency,
                ExtremelyUrgent = x.ExtremelyUrgent,
                ContentRequiresConfirmation = x.ContentRequiresConfirmation,
                DeadLine = x.DeadLine,
                SendToDesigner = x.SendToDesigner,
                FinalPublishingStrategy = x.FinalPublishingStrategy,
                PublishDate = x.PublishDate,
                PublishingStrategies = x.PublishingStrategies.Select(x => x.PublishingStrategy).ToList(),
                CurrentUser = x.CurrentUser == null ? null : new()
                {
                    DisplayName = x.CurrentUser.DisplayName,
                    Id = x.CurrentUserID!,
                    ReporterType = x.CurrentUser.ReporterType,
                }
            });
        }
        public static IQueryable<NewsMaterialDetailsVM> ToNewsMaterialDetailsVM(this IQueryable<NewsMaterial> source)
        {
            return source.Select(x => new NewsMaterialDetailsVM
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
                NewsRegion = x.NewsRegion,
                ReportedDate = x.ReportedDate,
                Type = x.Type,
                Urgency = x.Urgency,
                ExtremelyUrgent = x.ExtremelyUrgent,
                ContentRequiresConfirmation = x.ContentRequiresConfirmation,
                DeadLine = x.DeadLine,
                SendToDesigner = x.SendToDesigner,
                FinalPublishingStrategy = x.FinalPublishingStrategy,
                PublishDate = x.PublishDate,
                PublishingStrategies = x.PublishingStrategies.Select(x => x.PublishingStrategy).ToList(),
                CurrentUser = x.CurrentUser == null ? null : new()
                {
                    DisplayName = x.CurrentUser.DisplayName,
                    Id = x.CurrentUserID!,
                    ReporterType = x.CurrentUser.ReporterType,
                },
                Histories = x.Histories!.Select(h => new NewsMaterialHistoryVM
                {
                    ID = h.ID,
                    NewsUserID = h.NewsUserID,
                    NewsUserDisplayName = h.NewsUser!.DisplayName,
                    AssignerID = h.AssignerID,
                    AssignerDisplayName = h.Assigner == null ? null : h.Assigner.DisplayName,
                    Date = h.Date,
                    NewsMaterialID = h.NewsMaterialID,
                    Status = h.Status,
                }).ToList(),
                Logs = x.Logs.Select(a => new NewsMaterialLogVM
                {
                    ID = a.ID,
                    Log = a.Log,
                    NewsUserID = a.NewsUserID,
                    NewsUserDisplayName = a.NewsUser!.DisplayName,
                    NewsMaterialID = a.NewsMaterialID,
                }).ToList()
            });
        }
        public static IQueryable<NewsMaterialLiteVM> ToNewsMaterialLiteVM(this IQueryable<NewsMaterial> source)
        {
            return source.Select(x => new NewsMaterialLiteVM
            {
                ID = x.ID,
                Content = x.Content,
                CurrentStatus = x.CurrentStatus,
                Priority = x.Priority,
                ReportedDate = x.ReportedDate,
                Urgency = x.Urgency,
                ExtremelyUrgent = x.ExtremelyUrgent,
                ContentRequiresConfirmation = x.ContentRequiresConfirmation,
                DeadLine = x.DeadLine,
                Images = x.Images,
                CurrentUser = x.CurrentUser == null ? null : new()
                {
                    DisplayName = x.CurrentUser.DisplayName,
                    Id = x.CurrentUserID!,
                    ReporterType = x.CurrentUser.ReporterType
                }
            });
        }
        public static IQueryable<NewsUserVM> ToNewsUserVM(this IQueryable<NewsUser> source, IQueryable<IdentityUserClaim<string>> claims)
        {
            return source.GroupJoin(claims, x => x.Id, x => x.UserId, (user, cs) => new NewsUserVM
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email!,
                Image = user.Image,
                PhoneNumber = user.PhoneNumber,
                ReporterType = user.ReporterType,
                Roles = cs.Select(x => Enum.Parse<NewsUserRole>(x.ClaimValue!)).ToList()
            }).Where(x => x.Roles.Any());
        }
        public static IQueryable<NewsUserVM> ToNewsUserVMs(this IQueryable<NewsUser> source, IQueryable<IdentityUserClaim<string>> claims)
        {
            return (from u in source
                    from c in claims.Where(x => x.UserId == u.Id)
                    select new
                    {
                        User = new
                        {
                            u.Id,
                            u.DisplayName,
                            u.Email,
                            u.Image,
                            u.PhoneNumber,
                            u.ReporterType,
                        },
                        c.ClaimValue
                    }).GroupBy(x => x.User)
                      .Select(g => new NewsUserVM
                      {
                          Id = g.Key.Id,
                          DisplayName = g.Key.DisplayName,
                          Email = g.Key.Email,
                          Image = g.Key.Image,
                          PhoneNumber = g.Key.PhoneNumber,
                          ReporterType = g.Key.ReporterType,
                          Roles = g.Select(x => Enum.Parse<NewsUserRole>(x.ClaimValue!)).ToList()
                      });
        }
    }
}