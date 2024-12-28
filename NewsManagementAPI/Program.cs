using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewsManagementAPI.Data;
using NewsManagementAPI.Helpers;
using NewsManagementAPI.Models;
using NewsManagementAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<NewsDbContext>();

builder.Services.AddScoped<INotificationService, NotificationService>();

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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("cp", new OpenApiInfo { Title = "Control Panel", Version = "cp" });
    c.SwaggerDoc("fe", new OpenApiInfo { Title = "Mobile", Version = "fe" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
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

var app = builder.Build();
if (!builder.Environment.IsDevelopment())
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetService<NewsDbContext>();
    var userManager = serviceScope.ServiceProvider.GetService<UserManager<NewsUser>>();
    var memoryCache = serviceScope.ServiceProvider.GetService<IMemoryCache>();
    var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    dbContext?.Database.Migrate();

    MySeedInitializer.SeedData(userManager!, roleManager!, memoryCache!);
}

{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/cp/swagger.json", "Control Panel");
        c.SwaggerEndpoint("/swagger/fe/swagger.json", "Mobile");
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.MapControllers();

app.Run();
