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
    private readonly IProductService _productService;
    private readonly IProductVariantService _productvariantService;
    private readonly IProductImageService _productimageService;

    public ProductsController(IProductService productService, 
    IProductVariantService productVariantService,
    IProductImageService productImageService)
    {
        _productService = productService;
        _productvariantService = productVariantService;
        _productimageService = productImageService;
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

