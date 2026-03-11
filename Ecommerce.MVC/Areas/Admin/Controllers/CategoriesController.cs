using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.MVC.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerce.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // ─── INDEX (active list) ───────────────────────────────────────────────────
        // GET /Category
        // GET /Category?search=abc&pageNumber=2&pageSize=10&sortBy=name&sortDirection=asc
        public async Task<IActionResult> Index(CategoryFilterParams filter, CancellationToken ct)
        {
            filter.IsActive = null; // show all active (not deleted)

            var result = await _categoryService.GetPagedAsync(filter, ct);
            await PopulateParentSelectList(ct);

            ViewBag.Filter = filter;
            return View(result);
        }

        // ─── TRASH (soft-deleted list) ─────────────────────────────────────────────
        // GET /Category/Trash
        public async Task<IActionResult> Trash(CategoryFilterParams filter, CancellationToken ct)
        {
            var result = await _categoryService.GetDeletedAsync(filter, ct);
            ViewBag.Filter = filter;
            return View(result);
        }

        // ─── DETAIL ────────────────────────────────────────────────────────────────
        // GET /Category/Detail/5
        public async Task<IActionResult> Detail(int id, CancellationToken ct)
        {
            var dto = await _categoryService.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();
            return View(dto);
        }

        // ─── CREATE ────────────────────────────────────────────────────────────────
        // GET /Category/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateParentSelectList(ct);
            return View(new CreateCategoryDto { IsActive = true });
        }

        // POST /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentSelectList(ct);
                return View(dto);
            }

            var result = await _categoryService.CreateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await PopulateParentSelectList(ct);
                return View(dto);
            }

            TempData["Success"] = $"Category \"{result.Data!.Name}\" created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── EDIT ──────────────────────────────────────────────────────────────────
        // GET /Category/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var dto = await _categoryService.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();

            await PopulateParentSelectList(ct, dto.Id);
            return View(new UpdateCategoryDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Slug = dto.Slug,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ParentId = dto.ParentId,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            });
        }

        // POST /Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto dto, CancellationToken ct)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateParentSelectList(ct, dto.Id);
                return View(dto);
            }

            var result = await _categoryService.UpdateAsync(dto, ct);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error!);
                await PopulateParentSelectList(ct, dto.Id);
                return View(dto);
            }

            TempData["Success"] = $"Category \"{result.Data!.Name}\" updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ─── SOFT DELETE ───────────────────────────────────────────────────────────
        // GET /Category/SoftDelete/5  → confirmation page
        public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
        {
            var dto = await _categoryService.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();
            return View(dto);
        }

        // POST /Category/SoftDelete/5
        [HttpPost, ActionName("SoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _categoryService.SoftDeleteAsync(id, ct);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Index));
            }

            TempData["Warning"] = "Category moved to trash.";
            return RedirectToAction(nameof(Index));
        }

        // ─── HARD DELETE ───────────────────────────────────────────────────────────
        // GET /Category/HardDelete/5  → confirmation page (from Trash)
        public async Task<IActionResult> HardDelete(int id, CancellationToken ct)
        {
            // HardDelete works on already-soft-deleted items → query deleted list
            var deleted = await _categoryService.GetDeletedAsync(new CategoryFilterParams(), ct);
            var dto = deleted.Items.FirstOrDefault(c => c.Id == id);
            if (dto is null) return NotFound();
            return View(dto);
        }

        // POST /Category/HardDelete/5
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _categoryService.HardDeleteAsync(id, ct);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Trash));
            }

            TempData["Danger"] = "Category permanently deleted.";
            return RedirectToAction(nameof(Trash));
        }

        // ─── RESTORE ───────────────────────────────────────────────────────────────
        // POST /Category/Restore/5  (no GET page, direct action from Trash)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id, CancellationToken ct)
        {
            var result = await _categoryService.RestoreAsync(id, ct);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction(nameof(Trash));
            }

            TempData["Success"] = $"Category \"{result.Data!.Name}\" restored successfully.";
            return RedirectToAction(nameof(Trash));
        }

        // ─── HELPER ────────────────────────────────────────────────────────────────
        private async Task PopulateParentSelectList(CancellationToken ct, int? excludeId = null)
        {
            var all = await _categoryService.GetAllActiveAsync(ct);
            var list = all
                .Where(c => c.Id != excludeId)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();

            ViewBag.Parents = list;
        }
    }
}
