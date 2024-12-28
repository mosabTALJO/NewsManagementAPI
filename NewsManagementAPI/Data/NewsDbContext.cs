using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsManagementAPI.Models;
using System.Diagnostics;

namespace NewsManagementAPI.Data
{
    public class NewsDbContext : IdentityDbContext<NewsUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Debugger.IsAttached)
                optionsBuilder.UseNpgsql(@"Host=37.27.57.167; Port=5432; Database=Ecommerce; Username=ecomm; Password=aser@sWwer##4as");
            else
                optionsBuilder.UseNpgsql("Server=localhost; Database=News;User ID=newsadmin;Password=asw34##455@Awe;Encrypt=False");
        }
        public DbSet<NewsMaterial> NewsMaterials { get; set; }
        public DbSet<NewsMaterialHistory> NewsMaterialHistories { get; set; }
        public DbSet<NewsMaterialLog> NewsMaterialLogs { get; set; }
        public DbSet<SocialMedia> SocialMedias { get; set; }
        public DbSet<NewsSocialMedia> NewsSocialMedias { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public async Task CreateNewsMaterialHistory(int newsMaterialId, NewsMaterialStatus status, string userID, string? assignerID)
        {
            await NewsMaterialHistories.AddAsync(new()
            {
                NewsUserID = userID,
                NewsMaterialID = newsMaterialId,
                Date = DateTime.UtcNow,
                Status = status,
                AssignerID = assignerID
            });
        }
        public async Task CreateNewMaterialLog(int newsMaterialId, string userID, string log)
        {
            await NewsMaterialLogs.AddAsync(new()
            {
                Log = log,
                NewsUserID = userID,
                NewsMaterialID = newsMaterialId,
            });
        }
    }
}