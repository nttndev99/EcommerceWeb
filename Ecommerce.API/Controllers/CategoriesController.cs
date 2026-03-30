using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
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


}