using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.MVC.Filters;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure;
using Ecommerce.Application;
using Ecommerce.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Clean Architecture layer registrations
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// MVC with Filters
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
        options.Filters.Add<AdminLayoutFilter>();
    });

builder.Services.AddScoped<PerformanceLogAttribute>();
builder.Services.AddControllersWithViews();

// EF
builder.Services.AddDbContext<EcommerceDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:EcommerceConnection"]));

// Identity
builder.Services
    .AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();

// Cookie paths
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath        = "/Auth/Login";
    opt.LogoutPath       = "/Auth/Logout";
    opt.AccessDeniedPath = "/Auth/AccessDenied";
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin",    policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("CanManageProduct", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout      = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly  = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Migrate + Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db       = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    var logger   = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var services = scope.ServiceProvider;   // ← pass to seeder

    await db.Database.MigrateAsync();
    await AppDbSeeder.SeedAsync(db, logger, services);  // ← updated signature
}

// Pipeline
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
app.MapRazorPages();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();