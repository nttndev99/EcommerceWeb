using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Enums;
using Ecommerce.MVC.Areas.Admin.Models;
using Ecommerce.MVC.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService         _productService;
            private readonly ICategoryService        _categoryService;
            private readonly IProductVariantService  _variantService;

            public ProductsController(
                IProductService        productService,
                ICategoryService       categoryService,
                IProductVariantService variantService)
            {
                _productService  = productService;
                _categoryService = categoryService;
                _variantService  = variantService;
            }

            // ─── INDEX ─────────────────────────────────────────────────────────────────
            // GET /Product
            public async Task<IActionResult> Index(ProductFilterParams filter, CancellationToken ct)
            {
                var result = await _productService.GetPagedAsync(filter, ct);
                await PopulateCategoryList(ct);
                ViewBag.Filter = filter;
                return View(result);
            }

            // ─── TRASH ─────────────────────────────────────────────────────────────────
            // GET /Product/Trash
            public async Task<IActionResult> Trash(ProductFilterParams filter, CancellationToken ct)
            {
                var result = await _productService.GetDeletedAsync(filter, ct);
                ViewBag.Filter = filter;
                return View(result);
            }

            // ─── DETAIL ────────────────────────────────────────────────────────────────
            // GET /Product/Detail/5
            public async Task<IActionResult> Detail(int id, int variantPage = 1, CancellationToken ct = default)
            {
                var dto = await _productService.GetByIdAsync(id, ct);
                if (dto is null) return NotFound();

                var variants = await _variantService.GetByProductIdPagedAsync(
                    productId:  id,
                    pageNumber: variantPage,
                    pageSize:   10,
                    ct:         ct);
                    
                ViewBag.Variants = variants;
                return View(dto);
            }

            // ─── CREATE ────────────────────────────────────────────────────────────────
            // GET /Product/Create
            public async Task<IActionResult> Create(CancellationToken ct)
            {
                await PopulateCategoryList(ct);
                return View(new CreateProductDto { Status = ProductStatus.Active, BasePrice = 0 });
            }

            // // POST /Product/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(CreateProductDto dto, CancellationToken ct)
            {
                if (!ModelState.IsValid)
                {
                    await PopulateCategoryList(ct);
                    return View(dto);
                }

                var result = await _productService.CreateAsync(dto, ct);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, result.Error!);
                    await PopulateCategoryList(ct);
                    return View(dto);
                }

                TempData["Success"] = $"Product \"{result.Data!.Name}\" created successfully.";
                return RedirectToAction(nameof(Detail), new { id = result.Data.Id });
            }

            // ─── EDIT ──────────────────────────────────────────────────────────────────
            // GET /Product/Edit/5
            public async Task<IActionResult> Edit(int id, CancellationToken ct)
            {
                var dto = await _productService.GetByIdAsync(id, ct);
                if (dto is null) return NotFound();

                await PopulateCategoryList(ct);
                return View(new UpdateProductDto
                {
                    Id               = dto.Id,
                    Name             = dto.Name,
                    Slug             = dto.Slug,
                    Description      = dto.Description,
                    ShortDescription = dto.ShortDescription,
                    BasePrice        = dto.BasePrice,
                    SalePrice        = dto.SalePrice,
                    SKU              = dto.SKU,
                    Status           = dto.Status,
                    IsFeatured       = dto.IsFeatured,
                    CategoryId       = dto.CategoryId,
                    Brand            = dto.Brand,
                    Tags             = dto.Tags,
                    
                });
            }

            // // POST /Product/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, UpdateProductDto dto, CancellationToken ct)
            {
                if (id != dto.Id) return BadRequest();

                if (!ModelState.IsValid)
                {
                    await PopulateCategoryList(ct);
                    return View(dto);
                }

                var result = await _productService.UpdateAsync(dto, ct);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, result.Error!);
                    await PopulateCategoryList(ct);
                    return View(dto);
                }

                TempData["Success"] = $"Product \"{result.Data!.Name}\" updated successfully.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            // ─── SOFT DELETE ───────────────────────────────────────────────────────────
            // GET /Product/SoftDelete/5
            public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
            {
                var dto = await _productService.GetByIdAsync(id, ct);
                if (dto is null) return NotFound();
                return View(dto);
            }

            // POST /Product/SoftDelete/5
            [HttpPost, ActionName("SoftDelete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> SoftDeleteConfirmed(int id, CancellationToken ct)
            {
                var result = await _productService.SoftDeleteAsync(id, ct);
                TempData[result.IsSuccess ? "Warning" : "Error"] = result.IsSuccess
                    ? "Product moved to trash."
                    : result.Error;
                return RedirectToAction(nameof(Index));
            }

            // ─── HARD DELETE ───────────────────────────────────────────────────────────
            // GET /Product/HardDelete/5
            public async Task<IActionResult> HardDelete(int id, CancellationToken ct)
            {
                var deleted = await _productService.GetDeletedAsync(new ProductFilterParams(), ct);
                var dto = deleted.Items.FirstOrDefault(p => p.Id == id);
                if (dto is null) return NotFound();
                return View(dto);
            }

            // POST /Product/HardDelete/5
            [HttpPost, ActionName("HardDelete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> HardDeleteConfirmed(int id, CancellationToken ct)
            {
                var result = await _productService.HardDeleteAsync(id, ct);
                TempData[result.IsSuccess ? "Danger" : "Error"] = result.IsSuccess
                    ? "Product permanently deleted."
                    : result.Error;
                return RedirectToAction(nameof(Trash));
            }

            // ─── RESTORE ───────────────────────────────────────────────────────────────
            // POST /Product/Restore/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Restore(int id, CancellationToken ct)
            {
                var result = await _productService.RestoreAsync(id, ct);
                TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess
                    ? $"Product \"{result.Data!.Name}\" restored successfully."
                    : result.Error;
                return RedirectToAction(nameof(Trash));
            }

            // ─── HELPER ────────────────────────────────────────────────────────────────
            private async Task PopulateCategoryList(CancellationToken ct)
            {
                var cats = await _categoryService.GetAllActiveAsync(ct);
                ViewBag.Categories = cats
                    .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                    .ToList();
            }
    }
}