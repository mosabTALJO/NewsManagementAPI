using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NewsManagementAPI.Data;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddMemoryCache();
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    builder =>
    {
        builder.WithOrigins("*",
            "https://localhost:7190",
            "http://localhost:5125",
            "https://cp-ecommerce.usos.co",
            "https://Ecommerce.usos.co")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
var isDevelopment = builder.Environment.IsDevelopment();
if (!isDevelopment)
{
}

var app = builder.Build();
if (!isDevelopment)
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetService<NewsDbContext>();
    var userManager = serviceScope.ServiceProvider.GetService<UserManager<NewsUser>>();
    var memoryCache = serviceScope.ServiceProvider.GetService<IMemoryCache>();
    var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    dbContext?.Database.Migrate();

    MySeedInitializer.SeedData(userManager!, roleManager!, memoryCache!);
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

app.Run();
