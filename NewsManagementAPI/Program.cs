using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewsManagementAPI.Data;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

//Mustafa do we need both this and the one under it?
//builder.Services.AddScoped<DbContext, NewsDbContext>();
builder.Services.AddDbContext<NewsDbContext>();

#region Authentication
TokenValidationParameters tokenValidationParameters = new()
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidAudience = builder.Configuration["Jwt:Site"],
    ValidIssuer = builder.Configuration["Jwt:Site"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
};

builder.Services.AddIdentity<NewsUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
   .AddEntityFrameworkStores<NewsDbContext>()
   .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.AddAuthorization(option =>
{
    foreach (var r in Enum.GetValues<NewsUserRole>())
        option.AddPolicy(r.GetPolicyNumber(), policy => policy.AddRequirements(new AdminHasAccessRequirement(r.GetPolicyNumber())));
});
builder.Services.AddScoped<IAuthorizationHandler, AdminHasAccessHandler>();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParameters;
});
#endregion

//Mustafa I don't know this
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(
//    builder =>
//    {
//        builder.WithOrigins("*",
//            "http://localhost:4200",
//            "http://localhost:5173",
//            "https://cp-ecommerce.usos.co",
//            "https://Ecommerce.usos.co")
//               .AllowAnyMethod()
//               .AllowAnyHeader()
//               .AllowCredentials();
//    });
//});

var app = builder.Build();
if (!builder.Environment.IsDevelopment())
{
    using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var dbContext = serviceScope.ServiceProvider.GetService<NewsDbContext>();
        var userManager = serviceScope.ServiceProvider.GetService<UserManager<NewsUser>>();
        var memoryCache = serviceScope.ServiceProvider.GetService<IMemoryCache>();
        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        //var recurringJobManager = serviceScope.ServiceProvider.GetService<IRecurringJobManager>();
        dbContext?.Database.Migrate();

        MySeedInitializer.SeedData(userManager, roleManager, memoryCache);
        var options = serviceScope.ServiceProvider.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(options.Value);

        //if (!builder.Environment.IsDevelopment())
        //{
        //    recurringJobManager.RemoveIfExists("RefreshPrices");
        //    recurringJobManager.AddOrUpdate<CurrencyRateService>("RefreshPrices", x => x.RefreshRates(), Cron.Hourly(1));
        //}
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Mustafa do we need this?
//app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

app.Run();
