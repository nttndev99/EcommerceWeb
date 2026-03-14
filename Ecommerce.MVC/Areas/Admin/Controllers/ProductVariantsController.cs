using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers;
[Area("Admin")]
public class ProductVariantsController : Controller
{
    private readonly IProductVariantService _variantService;
    private readonly IProductService        _productService;

    public ProductVariantsController(
        IProductVariantService variantService,
        IProductService        productService)
    {
        _variantService = variantService;
        _productService = productService;
    }

    // ─── LIST (per product) ────────────────────────────────────────────────────
    // GET /ProductVariant/Index/5
    public async Task<IActionResult> Index(int productId, CancellationToken ct)
    {
        var variants = await _variantService.GetByProductIdAsync(productId, ct);

        var product = await _productService.GetByIdAsync(productId, ct);
        ViewBag.ProductId   = productId;
        ViewBag.ProductName = product?.Name ?? "Product";

        return View(variants);
    }

    // ─── CREATE ────────────────────────────────────────────────────────────────
    // GET /ProductVariant/Create?productId=5
    public async Task<IActionResult> Create(int productId, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        if (product is null) return NotFound();

        ViewBag.Product = product;
        return View(new CreateProductVariantDto
        {
            ProductId = productId,
            IsActive  = true,
            InitialStock     = 0
        });
    }

    // POST /ProductVariant/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductVariantDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Product = await _productService.GetByIdAsync(dto.ProductId, ct);
            return View(dto);
        }

        var result = await _variantService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Product = await _productService.GetByIdAsync(dto.ProductId, ct);
            return View(dto);
        }

        TempData["Success"] = $"Variant \"{result.Data!.Name}\" created.";
        return RedirectToAction(nameof(Index), new { productId = dto.ProductId });
    }

    // ─── EDIT ──────────────────────────────────────────────────────────────────
    // GET /ProductVariant/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await _variantService.GetByIdAsync(id, ct);
        if (dto is null) return NotFound();

        ViewBag.Product = await _productService.GetByIdAsync(dto.ProductId, ct);
        return View(new UpdateProductVariantDto
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            Name = dto.Name,
            SKU = dto.SKU,
            Price = dto.Price,
            SalePrice = dto.SalePrice,
            Color = dto.Color,
            Size = dto.Size,
            Material = dto.Material,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder, 
        });
    }

    // POST /ProductVariant/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductVariantDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Product = await _productService.GetByIdAsync(dto.ProductId, ct);
            return View(dto);
        }

        var result = await _variantService.UpdateAsync(dto, ct);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            ViewBag.Product = await _productService.GetByIdAsync(dto.ProductId, ct);
            return View(dto);
        }

        TempData["Success"] = "Variant updated successfully.";
        return RedirectToAction(nameof(Index), new { productId = dto.ProductId });
    }



        // ── SoftDelete ────────────────────────────────────────────────
    public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
    {
        var v = await _variantService.GetByIdAsync(id, ct);
        if (v is null) return NotFound();
        await PopulateProductInfo(v.ProductId, ct);
        return View(v);
    }

    [HttpPost, ActionName("SoftDelete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDeleteConfirmed(int id, CancellationToken ct)
    {
        var v         = await _variantService.GetByIdAsync(id, ct);
        int productId = v?.ProductId ?? 0;
        var result    = await _variantService.SoftDeleteAsync(id, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Variant \"{v?.Name}\" moved to trash."
            : result.Error;

        return RedirectToAction(nameof(Index), new { productId });
    }

    // ── Trash — theo productId ────────────────────────────────────
    public async Task<IActionResult> Trash(int id, CancellationToken ct)
    {
        await PopulateProductInfo(id, ct);
        var trashed = await _variantService.GetTrashedByProductIdAsync(id, ct);
        return View(trashed);
    }
    private async Task PopulateProductInfo(int productId, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(productId, ct);
        ViewBag.ProductId   = productId;
        ViewBag.ProductName = product?.Name ?? $"Product #{productId}";
    }
    // ── Restore ───────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int variantId, int productId, CancellationToken ct)
    {
        var trashed = await _variantService.GetTrashedByProductIdAsync(productId, ct);
        var name    = trashed.FirstOrDefault(v => v.Id == variantId)?.Name ?? $"#{variantId}";

        var result = await _variantService.RestoreAsync(variantId, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Variant \"{name}\" restored."
            : result.Error;

    return RedirectToAction(nameof(Trash), new { id = productId }); 
    }

    // ── HardDelete ────────────────────────────────────────────────
    public async Task<IActionResult> HardDelete(int variantId, int productId, CancellationToken ct)
    {
        var trashed = await _variantService.GetTrashedByProductIdAsync(productId, ct);
        var v       = trashed.FirstOrDefault(x => x.Id == variantId);
        if (v is null) return NotFound();

        await PopulateProductInfo(productId, ct);
        return View(v);
    }

    [HttpPost, ActionName("HardDelete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDeleteConfirmed(int variantId, int productId, CancellationToken ct)
    {
        var trashed = await _variantService.GetTrashedByProductIdAsync(productId, ct);
        var name    = trashed.FirstOrDefault(v => v.Id == variantId)?.Name ?? $"#{variantId}";

        var result = await _variantService.HardDeleteAsync(variantId, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? $"Variant \"{name}\" permanently deleted."
            : result.Error;

        return RedirectToAction(nameof(Trash), new { id = productId });
    }

    // ── EmptyTrash ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EmptyTrash(int productId, CancellationToken ct)
    {
        var result = await _variantService.EmptyTrashByProductAsync(productId, ct);

        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
            ? "Trash emptied."
            : result.Error;

    return RedirectToAction(nameof(Trash), new { id = productId }); 
    }
    



}