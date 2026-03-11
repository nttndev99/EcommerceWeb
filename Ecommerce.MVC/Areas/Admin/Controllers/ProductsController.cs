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
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IInventoryService inventoryService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // GET: /Products
        public async Task<IActionResult> Index([FromQuery] ProductFilterParams filter, CancellationToken ct)
        {
            var pagedResult = await _productService.GetPagedAsync(filter, ct);
            var categories = await _categoryService.GetAllActiveAsync(ct);
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync(ct);

            var vm = new ProductIndexViewModel
            {
                Products = pagedResult,
                Filter = filter,
                Categories = BuildCategorySelectList(categories),
                Statuses = BuildStatusSelectList(),
                LowStockCount = lowStockItems.Count()
            };

            return View(vm);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(id, ct);
            if (product == null) return NotFound();
            return View(new ProductDetailViewModel { Product = product });
        }

        // GET: /Products/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = await BuildFormViewModelAsync(null, ct);
            return View("Form", vm);
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModel]
        public async Task<IActionResult> Create(ProductFormViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View("Form", await RepopulateFormAsync(vm, ct));

            // Business validation: sale price < base price
            if (vm.SalePrice.HasValue && vm.SalePrice >= vm.BasePrice)
            {
                ModelState.AddModelError(nameof(vm.SalePrice), "Sale price must be less than base price.");
                return View("Form", await RepopulateFormAsync(vm, ct));
            }

            var dto = MapToCreateDto(vm);
            var result = await _productService.CreateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View("Form", await RepopulateFormAsync(vm, ct));
            }

            TempData["Success"] = $"Product '{result.Data!.Name}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = result.Data.Id });
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var product = await _productService.GetByIdAsync(id, ct);
            if (product == null) return NotFound();
            var vm = await BuildFormViewModelAsync(product, ct);
            return View("Form", vm);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateModel]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel vm, CancellationToken ct)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("Form", await RepopulateFormAsync(vm, ct));

            if (vm.SalePrice.HasValue && vm.SalePrice >= vm.BasePrice)
            {
                ModelState.AddModelError(nameof(vm.SalePrice), "Sale price must be less than base price.");
                return View("Form", await RepopulateFormAsync(vm, ct));
            }

            var dto = new UpdateProductDto { Id = vm.Id };
            MapToCreateDto(vm, dto);
            var result = await _productService.UpdateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                return View("Form", await RepopulateFormAsync(vm, ct));
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _productService.DeleteAsync(id, ct);
            TempData[result.IsSuccess ? "Success" : "Error"] =
                result.IsSuccess ? "Product deleted." : result.Error;
            return RedirectToAction(nameof(Index));
        }

        // ===== Private Helpers =====
        private async Task<ProductFormViewModel> BuildFormViewModelAsync(ProductDto? product, CancellationToken ct)
        {
            var categories = await _categoryService.GetAllActiveAsync(ct);
            if (product == null)
            {
                return new ProductFormViewModel
                {
                    Categories = BuildCategorySelectList(categories),
                    Statuses = BuildStatusSelectList()
                };
            }
            return new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                SKU = product.SKU,
                Status = product.Status,
                IsFeatured = product.IsFeatured,
                CategoryId = product.CategoryId,
                Brand = product.Brand,
                Tags = product.Tags,
                Categories = BuildCategorySelectList(categories),
                Statuses = BuildStatusSelectList()
            };
        }

        private async Task<ProductFormViewModel> RepopulateFormAsync(ProductFormViewModel vm, CancellationToken ct)
        {
            var categories = await _categoryService.GetAllActiveAsync(ct);
            vm.Categories = BuildCategorySelectList(categories);
            vm.Statuses = BuildStatusSelectList();
            return vm;
        }

        private static CreateProductDto MapToCreateDto(ProductFormViewModel vm, CreateProductDto? dto = null)
        {
            dto ??= new CreateProductDto();
            dto.Name = vm.Name;
            dto.Slug = vm.Slug;
            dto.Description = vm.Description;
            dto.ShortDescription = vm.ShortDescription;
            dto.BasePrice = vm.BasePrice;
            dto.SalePrice = vm.SalePrice;
            dto.SKU = vm.SKU;
            dto.Status = vm.Status;
            dto.IsFeatured = vm.IsFeatured;
            dto.CategoryId = vm.CategoryId;
            dto.Brand = vm.Brand;
            dto.Tags = vm.Tags;
            return dto;
        }

        private static IEnumerable<SelectListItem> BuildCategorySelectList(IEnumerable<CategoryDto> categories)
        {
            var items = new List<SelectListItem> { new("-- Select Category --", "") };
            items.AddRange(categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())));
            return items;
        }

        private static IEnumerable<SelectListItem> BuildStatusSelectList()
            => Enum.GetValues<ProductStatus>().Select(s => new SelectListItem(s.ToString(), ((int)s).ToString()));
    }
}