using System.Text;
using Ecommerce.API.Extensions;
using Ecommerce.API.Middleware;
using Ecommerce.Application;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Clean Architecture layers ─────────────────────────────────
// Chỉ gọi MỘT LẦN — DbContext đã được đăng ký bên trong đây
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy   = System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


// ── Identity ──────────────────────────────────────────────────
builder.Services
    .AddIdentity<AppUser, IdentityRole>(opts =>
    {
        opts.Password.RequireDigit           = true;
        opts.Password.RequiredLength         = 6;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase       = false;
        opts.User.RequireUniqueEmail         = true;
        opts.SignIn.RequireConfirmedEmail     = false;
    })
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();

// ── JWT ───────────────────────────────────────────────────────
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()!;

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew                = TimeSpan.Zero,
        };

        opts.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"success":false,"message":"Unauthorized. Please provide a valid token."}""");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"success":false,"message":"Forbidden. You do not have permission."}""");
            },
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("RequireAdmin",    p => p.RequireRole("Admin"));
    opts.AddPolicy("RequireCustomer", p => p.RequireRole("Customer"));
});

// ── Application Services ──────────────────────────────────────
builder.Services.AddScoped<IAuthService,         AuthService>();
builder.Services.AddScoped<IEmailService,        EmailService>();
builder.Services.AddScoped<IOrderService,        OrderService>();
builder.Services.AddScoped<ICustomerService,     CustomerService>();
builder.Services.AddScoped<IIdentityService,     IdentityService>();
builder.Services.AddScoped<IRolesService,        RolesService>();
builder.Services.AddScoped<IUsersManagerService, UsersManagerService>();
builder.Services.AddScoped<IInventoryService,    InventoryService>();

builder.Services.AddHttpContextAccessor();

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// ── Swagger ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Ecommerce API",
        Version     = "v1",
        Description = "RESTful API for Ecommerce platform",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your token}",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Migrate + Seed ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db       = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    var logger   = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var services = scope.ServiceProvider;
    await db.Database.MigrateAsync();
    await AppDbSeeder.SeedAsync(db, logger, services);
}

// ── Pipeline ──────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();