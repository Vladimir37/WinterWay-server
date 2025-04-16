using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WinterWay.Data;
using WinterWay.Enums;
using WinterWay.Filters;
using WinterWay.Middlewares;
using WinterWay.Models.Database.Auth;
using WinterWay.Models.DTOs.Responses.Shared;
using WinterWay.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var webFrontendUrl = builder.Configuration.GetValue<string>("WebFrontendURL");

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(webFrontendUrl!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "WinterWayAuth";
    options.Cookie.HttpOnly = false;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new ApiErrorDTO(InternalError.NotAuthorized, "User is not authenticated"));
        }
    };
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilter>();
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddScoped<RollService>();
builder.Services.AddScoped<CompleteTaskService>();
builder.Services.AddScoped<DateTimeService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<DiaryService>();
builder.Services.AddScoped<TimerService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<BackupService>();
builder.Services.AddSingleton<RateLimiterService>();
builder.Services.AddSingleton<BackgroundImageService>();
builder.Services.AddHostedService<StartupService>();

var app = builder.Build();

//Uncomment when https will be ready
//app.UseHttpsRedirection();

app.UseCors("AllowFrontendApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers()
    .RequireAuthorization(); 

app.Run();
