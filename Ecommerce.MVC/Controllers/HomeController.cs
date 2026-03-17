using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.MVC.Models;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IUnitOfWork  _uow;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    public HomeController(IUnitOfWork uow, ICartService cartService, IProductService productService, ICategoryService categoryService)
    {
        _uow         = uow;
        _cartService = cartService;
        _productService = productService;
        _categoryService = categoryService;
    }

    // ─────────────────────────────────────────────
    // GET /
    // ─────────────────────────────────────────────
    public async Task<IActionResult> Index(string tab = "best", CancellationToken ct = default)
    {
        var products = await _productService.GetHomeProducts(tab, ct);
        var categories = await _categoryService.GetHomeCategories(ct);

        var vm = new HomeViewModel
        {
            Products   = products,
            Categories = categories,
            ActiveTab = tab

        };

        return View(vm);
    }
    public async Task<IActionResult> LoadProducts(string tab, CancellationToken ct)
    {
        var products = await _productService.GetHomeProducts(tab, ct);

        return PartialView("_HomeProductList", products);
    }
    // ─────────────────────────────────────────────
    // GET /Home/ProductDetail/{id}
    // ─────────────────────────────────────────────
    public async Task<IActionResult> ProductDetail(int id, CancellationToken ct)
    {
        var product = await _uow.Products.GetWithDetailsAsync(id, ct);

        if (product is null || product.Status != ProductStatus.Active || product.IsDeleted)
            return NotFound();

        var related = await _uow.Products.Query()
            .Where(r => r.CategoryId == product.CategoryId &&
                        r.Id != id &&
                        r.Status == ProductStatus.Active &&
                        !r.IsDeleted)
            .Include(r => r.Images)
            .Include(r => r.Inventories)
            .OrderByDescending(r => r.CreatedAt)
            .Take(4)
            .ToListAsync(ct);

        ViewBag.Related = related.Select(MapProduct).ToList();
        return View(MapProductDetail(product));
    }

    // ─────────────────────────────────────────────
    // GET /Home/Category/{slug}
    // ─────────────────────────────────────────────
    public async Task<IActionResult> Category(
        string slug, int page = 1, CancellationToken ct = default)
    {
        var category = await _uow.Categories.Query()
            .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted, ct);

        if (category is null) return NotFound();

        const int pageSize = 12;

        var all = await _uow.Products.Query()
            .Where(p => p.CategoryId == category.Id &&
                        p.Status == ProductStatus.Active &&
                        !p.IsDeleted)
            .Include(p => p.Images)
            .Include(p => p.Inventories)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        ViewBag.Category    = MapCategory(category);
        ViewBag.Products    = all.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(MapProduct).ToList();
        ViewBag.TotalCount  = all.Count;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages  = (int)Math.Ceiling((double)all.Count / pageSize);

        return View();
    }

    // ─────────────────────────────────────────────
    // MAPPERS
    // ─────────────────────────────────────────────
    private static ProductListDto MapProduct(Ecommerce.Domain.Entities.Product p) => new()
    {
        Id              = p.Id,
        Name            = p.Name,
        Slug            = p.Slug,
        BasePrice       = p.BasePrice,
        SalePrice       = p.SalePrice,
        SKU             = p.SKU,
        Status          = p.Status,
        IsFeatured      = p.IsFeatured,
        CategoryName    = p.Category?.Name ?? string.Empty,
        Brand           = p.Brand,
        PrimaryImageUrl = p.Images?
            .OrderBy(i => i.DisplayOrder)
            .FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
        TotalStock   = p.Inventories?.Sum(i => i.AvailableQuantity) ?? 0,
        VariantCount = p.Variants?.Count ?? 0,
        CreatedAt    = p.CreatedAt,
    };

    private static ProductDto MapProductDetail(Ecommerce.Domain.Entities.Product p) => new()
    {
        Id               = p.Id,
        Name             = p.Name,
        Slug             = p.Slug,
        Description      = p.Description,
        ShortDescription = p.ShortDescription,
        BasePrice        = p.BasePrice,
        SalePrice        = p.SalePrice,
        SKU              = p.SKU,
        Status           = p.Status,
        IsFeatured       = p.IsFeatured,
        CategoryId       = p.CategoryId,
        CategoryName     = p.Category?.Name ?? string.Empty,
        Brand            = p.Brand,
        Tags             = p.Tags,
        PrimaryImageUrl  = p.Images?
            .OrderBy(i => i.DisplayOrder)
            .FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
        TotalStock   = p.Inventories?.Sum(i => i.AvailableQuantity) ?? 0,
        VariantCount = p.Variants?.Count ?? 0,
        CreatedAt    = p.CreatedAt,
        Images       = p.Images?
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto
            {
                Id           = i.Id,
                ProductId    = i.ProductId,
                ImageUrl     = i.ImageUrl,
                AltText      = i.AltText,
                IsPrimary    = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
            }).ToList() ?? new(),
        Variants     = p.Variants?
            .Where(v => v.IsActive)
            .OrderBy(v => v.DisplayOrder)
            .Select(v => new ProductVariantDto
            {
                Id           = v.Id,
                ProductId    = v.ProductId,
                Name         = v.Name,
                SKU          = v.SKU,
                Price        = v.Price,
                SalePrice    = v.SalePrice,
                Color        = v.Color,
                ColorHex     = v.ColorHex,
                Size         = v.Size,
                Material     = v.Material,
                ImageUrl     = v.ImageUrl,
                IsActive     = v.IsActive,
                DisplayOrder = v.DisplayOrder,
                Stock        = p.Inventories?
                    .FirstOrDefault(i => i.ProductVariantId == v.Id)
                    ?.AvailableQuantity ?? 0,
            }).ToList() ?? new(),
    };

    private static CategoryListDto MapCategory(Ecommerce.Domain.Entities.Category c) => new()
    {
        Id           = c.Id,
        Name         = c.Name,
        Slug         = c.Slug,
        IsActive     = c.IsActive,
        DisplayOrder = c.DisplayOrder,
        ProductCount = c.Products?.Count(p => !p.IsDeleted) ?? 0,
    };
}