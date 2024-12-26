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
                optionsBuilder.UseSqlServer("Server=localhost; Database=News;User ID=newsadmin;Password=asw34##455@Awe;Encrypt=False");
            //"server=localhost;database=MasjedDB;User ID=Masjed;password=cbffe2404c29A@1;MultipleActiveResultSets=True;Encrypt=False"
        }
        public DbSet<NewsMaterial> NewsMaterials { get; set; }
        public DbSet<NewsMaterialHistory> NewsMaterialHistories { get; set; }
        public DbSet<NewsMaterialLog> NewsMaterialLogs { get; set; }
        public DbSet<SocialMedia> SocialMedias { get; set; }
        public DbSet<NewsSocialMedia> NewsSocialMedias { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
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