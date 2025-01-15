using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;
using NewsManagementAPI.Models;
using NewsManagementAPI.Data;

namespace NewsManagementAPI.Services
{
    public interface INotificationService
    {
        Task AddNotification(Notification notification, DateTime? ScheduledDate = null);
        //Task SendN
    }
    public class NotificationService : INotificationService
    {
        readonly NewsDbContext newsDbContext;
        public NotificationService(NewsDbContext newsDbContext)
        {
            this.newsDbContext = newsDbContext;
        }
        public async Task AddNotification(Notification notification, DateTime? ScheduledDate = null)
        {
            if (FirebaseApp.DefaultInstance == null)
                FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebasekey.json")) });

            var messaging = FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance;
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Notification = new FirebaseNotification
                {
                    Title = notification.Title,
                    Body = notification.Text,
                    ImageUrl = notification.Image,
                },
                Data = new Dictionary<string, string?>()
                {
                    ["title"] = notification.Title,
                    ["text"] = notification.Text,
                    ["image"] = notification.Image,
                    ["url"] = notification.Url,
                },
                Topic = notification.Topic,
                Apns = new FirebaseAdmin.Messaging.ApnsConfig
                {
                    Aps = new FirebaseAdmin.Messaging.Aps { MutableContent = true },
                    FcmOptions = new FirebaseAdmin.Messaging.ApnsFcmOptions { ImageUrl = notification.Image }
                },
            };
            await newsDbContext.Notifications.AddAsync(notification);
            await newsDbContext.SaveChangesAsync();

            if (ScheduledDate is null)
                await messaging.SendAsync(message);
        }
    }
}