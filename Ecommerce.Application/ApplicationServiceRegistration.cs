
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Services;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductVariantService, ProductVariantService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddScoped<IIdentityService, IdentityService>();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IUsersManagerService, UsersManagerService>();

        services.AddScoped<ICartService, CartService>();
        return services;
    }
}