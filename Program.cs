using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;// Thêm cho Razor Runtime Compilation
using LMS.Data;
using LMS.Data.Entities;
using Serilog;
using LMS.Services;
using LMS.ViewModels.VNPay;
using LMS.Core;
using LMS.Core.Repositories;
using LMS.Repositories;
using LMS.ViewModels;
using LMS.Hubs;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SQLServerConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Sử dụng AddIdentity thay vì AddDefaultIdentity để tránh cảnh báo
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Thêm các cấu hình password để tránh cảnh báo
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
// Thêm Razor Runtime Compilation để khắc phục vấn đề với RazorSourceGenerator
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Giúp tránh cảnh báo CS8785

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation(); // Giúp tránh cảnh báo CS8785
#region Chatbot

#endregion
builder.Services.AddTransient<IStorageService, FileStorageService>();
builder.Services.AddSingleton<IVnPayService, VnPayService>();
builder.Services.Configure<VnPayConfigOptions>(
builder.Configuration.GetSection("VnPay"));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSenderService>();
#region Authorization

AddAuthorizationPolicies();

#endregion
builder.Services.AddSignalR();
AddScoped();
var app = builder.Build();
app.MapHub<ChatHub>("/chatHub");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HTTP Strict Transport Security value is 30 days.
    // This is a security enhancement to prevent man-in-the-middle attacks.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        await DbInitializer.CreateRoles(serviceProvider, userManager);
        Log.Information("Seeding data...");
        // Sử dụng GetRequiredService thay vì GetService để tránh cảnh báo null check
        var dbInitializer = serviceProvider.GetRequiredService<DbInitializer>();
        dbInitializer.Seed().Wait();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

void AddAuthorizationPolicies()
{
    // Sử dụng AddAuthorizationBuilder để tránh cảnh báo
    var authBuilder = builder.Services.AddAuthorizationBuilder();

    // Thêm các policy
    authBuilder.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
    authBuilder.AddPolicy(Constants.Policies.RequireAdmin, policy => policy.RequireRole(Constants.Roles.Administrator));
    authBuilder.AddPolicy(Constants.Policies.RequireManager, policy => policy.RequireRole(Constants.Roles.Manager));
}
#region Scoped
void AddScoped()
{
    builder.Services.AddScoped<DbInitializer>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IPostService, PostService>();
}
#endregion