using ASC.DataAccess;
using ASC.Solution.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConfig(builder.Configuration);
builder.Services.AddMyDependencyGroup();


// Cấu hình logging
builder.Logging.AddConsole();

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Thêm DbContext vào DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Cấu hình session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Đăng ký các service
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Cấu hình ApplicationSettings
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();
    await storageSeed.Seed(
        scope.ServiceProvider.GetService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetService<IOptions<ApplicationSettings>>()
     ).ConfigureAwait(false);
}


// Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Đừng quên bật session!

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var navigationCacheOperations = scope.ServiceProvider.GetRequiredService<InavigationCacheOperations>();
    await navigationCacheOperations.CreateNavigationCacheAsync();
}
app.Run();



