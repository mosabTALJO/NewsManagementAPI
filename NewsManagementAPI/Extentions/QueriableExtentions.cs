using NewsManagementAPI.Models;
using NewsManagementAPI.ViewModels;

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
                PublishingStrategies = x.PublishingStrategies.Select(x => x.PublishingStrategy).ToList(),
                Histories = x.Histories!.Select(h => new NewsMaterialHistoryVM
                {
                    AssignerID = h.AssignerID,
                    Date = h.Date,
                    NewsUserID = h.NewsUserID,
                    ID = h.ID,
                    NewsMaterialID = h.NewsMaterialID,
                    Status = h.Status,
                }).ToList(),
                CurrentUser = x.CurrentUser == null ? null : new() { DisplayName = x.CurrentUser.DisplayName, Id = x.CurrentUserID! }
            });
        }
        public static IQueryable<NewsUserVM> ToNewsUserVM(this IQueryable<NewsUser> source)
        {
            return source.Select(x => new NewsUserVM
            {
                DisplayName = x.DisplayName,
                Email = x.Email!,
                Image = x.Image,
                PhoneNumber = x.PhoneNumber,
            });
        }
    }
}
