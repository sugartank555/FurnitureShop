using FurnitureShop.Data;
using FurnitureShop.Models;
using FurnitureShop.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Kết nối SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2️⃣ Cấu hình Identity (hỗ trợ Roles)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3️⃣ Cookie redirect
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 4️⃣ Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 5️⃣ Payment Service (Momo)
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// 6️⃣ MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ✅ BUILD APP (sau khi đăng ký tất cả services)
var app = builder.Build();

// 7️⃣ Pipeline
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
app.UseAuthentication();
app.UseAuthorization();

// 8️⃣ Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// 9️⃣ Seed tài khoản Admin & Support
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Danh sách role cần tạo
    string[] roleNames = { "Admin", "Support" };

    // Tạo role nếu chưa có
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 🧑‍💼 1️⃣ Admin
    string adminEmail = "admin@shop.com";
    string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản trị viên",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // 👩‍💻 2️⃣ Nhân viên CSKH
    string supportEmail = "support@shop.com";
    string supportPassword = "Support@123";

    var supportUser = await userManager.FindByEmailAsync(supportEmail);
    if (supportUser == null)
    {
        supportUser = new ApplicationUser
        {
            UserName = supportEmail,
            Email = supportEmail,
            FullName = "Nhân viên CSKH",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(supportUser, supportPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(supportUser, "Support");
    }
}

app.Run();