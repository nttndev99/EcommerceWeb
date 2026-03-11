using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.MVC.Filters;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure;
using Ecommerce.Application;

var builder = WebApplication.CreateBuilder(args);
// Clean Architecture layer registrations
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// MVC with Filters
builder.Services
    .AddControllersWithViews(options =>
    {
        // Global exception filter
        options.Filters.Add<GlobalExceptionFilter>();
        // Global layout data filter
        options.Filters.Add<AdminLayoutFilter>();
    });

// Register filters for DI
builder.Services.AddScoped<PerformanceLogAttribute>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF
builder.Services.AddDbContext<EcommerceDbContext>(opts => {
    opts.UseSqlServer(builder.Configuration["ConnectionStrings:EcommerceConnection"]);
});
// Auth
builder.Services
    .AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireCustomer", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("CanManageProduct", policy =>
        policy.RequireRole("Admin"));
});


builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// ── Migrate + Seed ────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await db.Database.MigrateAsync();
    await AppDbSeeder.SeedAsync(db, logger);
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapRazorPages();
app.UseSession();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
