using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.API.Controllers;

// ═══════════════════════════════════════════════════════════════
// PRODUCTS
// ═══════════════════════════════════════════════════════════════
public class ProductsController : BaseApiController
{
    // FIX 1: Xóa khai báo duplicate _productService (compile error)
    private readonly IProductService _productService;
    private readonly IProductVariantService _productvariantService;
    private readonly IProductImageService _productimageService;
    private readonly ICategoryService _categoryService;
    private readonly IInventoryService _inventoryService;

    public ProductsController(IProductService productService, 
    IProductVariantService productVariantService,
    IProductImageService productImageService,
    ICategoryService categoryService,
    IInventoryService inventoryService)
    {
        _productService = productService;
        _productvariantService = productVariantService;
        _productimageService = productImageService;
        _categoryService = categoryService;
        _inventoryService = inventoryService;
    }

    /// <summary>Get paged list of products</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProductFilterParams filter, CancellationToken ct)
    {
        var result = await _productService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    /// <summary>Get product by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return product is null
            ? NotFound($"Product {id} not found.")
            : Ok(product);
    }

    /// <summary>Get product by slug</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, ProductFilterParams filter, CancellationToken ct)
    {
        var product = await _productService.GetBySlugAsync(slug, filter, ct);
        return product is null
            ? NotFound($"Product '{slug}' not found.")
            : Ok(product);
    }

    /// <summary>Create product [Admin]</summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest("Validation failed.");

        var result = await _productService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Created($"/api/v1/products/{result.Data!.Id}", result.Data, "Product created.")
            : BadRequest(result.Error!);
    }

    /// <summary>Update product [Admin]</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return BadRequest("ID mismatch.");
        if (!ModelState.IsValid) return BadRequest("Validation failed.");

        var result = await _productService.UpdateAsync(dto, ct);
        return FromResult(result, "Product updated.");
    }

    /// <summary>Delete product [Admin]</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _productService.HardDeleteAsync(id, ct);
        return FromResult(result, "Product deleted.");
    }

    // ── Variants ───────────────────────────────────────────────
    [HttpGet("{id:int}/variants")]
    public async Task<IActionResult> GetVariants(int id, CancellationToken ct)
    {
        var variants = await _productvariantService.GetByProductIdAsync(id, ct);
        return Ok(variants);
    }

    [HttpPost("{id:int}/variants")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> AddVariant(
        int id, [FromBody] CreateProductVariantDto dto, CancellationToken ct)
    {
        dto.ProductId = id;
        var result = await _productvariantService.CreateAsync(dto, ct);
        return result.IsSuccess
            // FIX 2: result.Value → result.Data
            ? Created($"/api/v1/products/{id}/variants", result.Data, "Variant added.")
            : BadRequest(result.Error!);
    }

    [HttpPut("{id:int}/variants/{variantId:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateVariant(
        int id, int variantId, [FromBody] UpdateProductVariantDto dto, CancellationToken ct)
    {
        dto.ProductId = id;
        dto.Id        = variantId;
        var result = await _productvariantService.UpdateAsync(dto, ct);
        return FromResult(result, "Variant updated.");
    }

    [HttpDelete("{id:int}/variants/{variantId:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteVariant(
        int id, int variantId, CancellationToken ct)
    {
        var result = await _productService.HardDeleteAsync(variantId, ct);
        return FromResult(result, "Variant deleted.");
    }

    // ── Images ─────────────────────────────────────────────────
    [HttpGet("{id:int}/images")]
    public async Task<IActionResult> GetImages(int id, CancellationToken ct)
    {
        var images = await _productimageService.GetByProductIdAsync(id, ct);
        return Ok(images);
    }

    [HttpPost("{id:int}/images")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> AddImage(
        int id, [FromBody] CreateProductImageDto dto, CancellationToken ct)
    {
        dto.ProductId = id;
        var result = await _productimageService.CreateAsync(dto, ct);
        return result.IsSuccess
            // FIX 3: result.Value → result.Data
            ? Created($"/api/v1/products/{id}/images", result.Data, "Image added.")
            : BadRequest(result.Error!);
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteImage(
        int id, int imageId, CancellationToken ct)
    {
        var result = await _productimageService.DeleteAsync(imageId, ct);
        return FromResult(result, "Image deleted.");
    }
}

// ═══════════════════════════════════════════════════════════════
// CATEGORIES
// ═══════════════════════════════════════════════════════════════
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
        => _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryFilterParams filter, CancellationToken ct)
    {
        var result = await _categoryService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var cat = await _categoryService.GetByIdAsync(id, ct);
        return cat is null ? NotFound($"Category {id} not found.") : Ok(cat);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] CategoryFilterParams filter, CancellationToken ct)
    {
        var cat = await _categoryService.GetBySlugAsync(slug, filter, ct);
        return cat is null ? NotFound($"Category '{slug}' not found.") : Ok(cat);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Validation failed.");
        var result = await _categoryService.CreateAsync(dto, ct);
        return result.IsSuccess
            // FIX 4: result.Value → result.Data (nhất quán với toàn bộ codebase)
            ? Created($"/api/v1/categories/{result.Data!.Id}", result.Data, "Category created.")
            : BadRequest(result.Error!);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return BadRequest("ID mismatch.");
        var result = await _categoryService.UpdateAsync(dto, ct);
        return FromResult(result, "Category updated.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _categoryService.HardDeleteAsync(id, ct);
        return FromResult(result, "Category deleted.");
    }
}

// ═══════════════════════════════════════════════════════════════
// INVENTORY
// ═══════════════════════════════════════════════════════════════
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
        => _inventoryService = inventoryService;

    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] InventoryFilterParams filter, CancellationToken ct)
    {
        var result = await _inventoryService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var inv = await _inventoryService.GetByIdAsync(id, ct);
        return inv is null ? NotFound($"Inventory {id} not found.") : Ok(inv);
    }

    [HttpGet("low-stock")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> LowStock(
        [FromQuery] InventoryFilterParams filter, CancellationToken ct)
    {
        filter.IsLowStock = true;
        var result = await _inventoryService.GetPagedAsync(filter, ct);
        return Paged(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateInventoryDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _inventoryService.UpdateStockAsync(dto, ct);
        return FromResult(result, "Inventory updated.");
    }

    [HttpPost("{id:int}/adjust")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Adjust(
        int id, [FromBody] AdjustInventoryDto dto, CancellationToken ct)
    {
        dto.Id = id;
        var result = await _inventoryService.AdjustAsynckAsync(dto, ct);
        return FromResult(result, "Inventory adjusted.");
    }
}