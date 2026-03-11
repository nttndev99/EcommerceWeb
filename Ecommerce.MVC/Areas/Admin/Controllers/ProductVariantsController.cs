using Ecommerce.Application.DTOs.Product;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductVariantsController : Controller
    {
        private readonly IProductVariantService _variantService;
        private readonly IProductService _productService;

        public ProductVariantsController(IProductVariantService variantService, IProductService productService)
        {
            _variantService = variantService;
            _productService = productService;
        }

        // GET: /ProductVariants?ProductId=1
        public async Task<IActionResult> Index([FromQuery] ProductVariantFilterParams filter, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(filter.ProductId, ct);
            if (product == null) return NotFound();
            var variants = await _variantService.GetPagedByProductAsync(filter, ct);
            ViewBag.Product = product;
            ViewBag.Filter = filter;
            return View(variants);
        }

        // GET: /ProductVariants/Create?productId=1
        public async Task<IActionResult> Create(int productId, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(productId, ct);
            if (product == null) return NotFound();
            ViewBag.Product = product;
            return View(new CreateProductVariantDto { ProductId = productId });
        }

        // POST: /ProductVariants/Create
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
            TempData["Success"] = "Variant created.";
            return RedirectToAction("Details", "Products", new { id = dto.ProductId });
        }

        // GET: /ProductVariants/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var variant = await _variantService.GetByIdAsync(id, ct);
            if (variant == null) return NotFound();
            var product = await _productService.GetByIdAsync(variant.ProductId, ct);
            ViewBag.Product = product;
            return View(new UpdateProductVariantDto
            {
                Id = variant.Id, ProductId = variant.ProductId, Name = variant.Name,
                SKU = variant.SKU, Price = variant.Price, SalePrice = variant.SalePrice,
                Color = variant.Color, Size = variant.Size, Material = variant.Material,
                ImageUrl = variant.ImageUrl, IsActive = variant.IsActive, DisplayOrder = variant.DisplayOrder
            });
        }

        // POST: /ProductVariants/Edit/5
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
                return View(dto);
            }
            TempData["Success"] = "Variant updated.";
            return RedirectToAction("Details", "Products", new { id = dto.ProductId });
        }

        // POST: /ProductVariants/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int productId, CancellationToken ct)
        {
            await _variantService.DeleteAsync(id, ct);
            TempData["Success"] = "Variant deleted.";
            return RedirectToAction("Details", "Products", new { id = productId });
        }
    }
}