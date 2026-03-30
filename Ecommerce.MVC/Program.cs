using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.MVC.Filters;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure;
using Ecommerce.Application;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.UnitOfWork;
using Ecommerce.Infrastructure.Services;
using Ecommerce.Application.Interfaces;
using Ecommerce.MVC.ViewComponents;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Clean Architecture layers ─────────────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration); // <---- CALL builder.Services.AddDbContext<EcommerceDbContext>, EF + UoW
// ── MVC + Filters ─────────────────────────────────────────────────────────
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
        options.Filters.Add<AdminLayoutFilter>();
        options.Filters.Add<CartLayoutFilter>();   // ← inject cart count vào ViewBag
    });

builder.Services.AddScoped<PerformanceLogAttribute>();

// ── EF + UoW ─────────────────────────────────────────────────────────────
// builder.Services.AddDbContext<EcommerceDbContext>(opts =>
//     opts.UseSqlServer(builder.Configuration["ConnectionStrings:EcommerceConnection"]));

// builder.Services.AddScoped<UnitOfWork>();
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();  

// ── Identity ─────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Password policy
        options.Password.RequireDigit           = true;
        options.Password.RequiredLength         = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase       = false;
        // Lockout
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers      = true;
        // User
        options.User.RequireUniqueEmail         = true;
        // Sign-in
        options.SignIn.RequireConfirmedEmail     = false; // set true khi SMTP ready
    })
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();

// ── Cookie config ─────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath        = "/Auth/Login";
    opt.LogoutPath       = "/Auth/Logout";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
    opt.ExpireTimeSpan   = TimeSpan.FromDays(7);
    opt.SlidingExpiration = true;
});

// ── Authorization policies ────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin",     policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomer",  policy => policy.RequireRole("Customer"));
    options.AddPolicy("CanManageProduct", policy => policy.RequireRole("Admin"));
});

// ── Infrastructure services ───────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,         AuthService>();
builder.Services.AddScoped<IEmailService,        EmailService>();
builder.Services.AddScoped<ICartService,         CartService>();
builder.Services.AddScoped<IOrderService,        OrderService>();
builder.Services.AddScoped<ICustomerService,     CustomerService>();
builder.Services.AddScoped<IIdentityService,     IdentityService>();
builder.Services.AddScoped<IRolesService,        RolesService>();
builder.Services.AddScoped<IUsersManagerService, UsersManagerService>();
builder.Services.AddScoped<IInventoryService,    InventoryService>();

// ── ViewComponents ────────────────────────────────────────────────────────
builder.Services.AddScoped<CartBadgeViewComponent>();

// ── Infrastructure ────────────────────────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout       = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly   = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name       = ".Ecommerce.Session";
});

var app = builder.Build();

// ── Migrate + Seed ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db       = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    var logger   = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var services = scope.ServiceProvider;

    await db.Database.MigrateAsync();
    await AppDbSeeder.SeedAsync(db, logger, services);
}

// ── Pipeline ──────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();       
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();