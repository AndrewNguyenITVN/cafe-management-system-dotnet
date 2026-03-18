using CafeManagement.Data;
using CafeManagement.Hubs; // Thêm thư viện Hub mới
using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── Cookie Auth ───────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── POS Session (Cookie riêng cho kiosk POS) ──────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".PosSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// ── Services (DI) ─────────────────────────────────────────
// Master Data Services (TV1)
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<MenuItemService>();
builder.Services.AddScoped<ToppingService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<JobPositionService>();
builder.Services.AddScoped<UserService>();
// TV3: Inventory & Recipe
builder.Services.AddScoped<RecipeService>();
// POS / CRM / Inventory Services (TV2, TV3, TV5 implement)
builder.Services.AddScoped<IOrderService, OrderService>(); // TV2: POS Order
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PointService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ShiftHandoverService>();
builder.Services.AddScoped<ReportingService>();

// ── MVC & SIGNALR ──────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); // MỚI: Đăng ký dịch vụ phát sóng Real-time

var app = builder.Build();

// ── Seed Admin user ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await SeedAdminAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ───────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Bật POS session (đặt trước UseAuthentication)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// MỚI: Mở cổng "/orderHub" để các trình duyệt kết nối WebSockets
app.MapHub<OrderHub>("/orderHub");

app.Run();

// ── Seed helper ───────────────────────────────────────────
static async Task SeedAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();

    // Tạo Roles
    string[] roles = { "Admin", "Manager", "Staff" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Tạo tài khoản Admin mặc định
    const string adminEmail = "admin@cafe.com";
    const string adminPassword = "Admin@123";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản trị viên",
            IsActive = true,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
