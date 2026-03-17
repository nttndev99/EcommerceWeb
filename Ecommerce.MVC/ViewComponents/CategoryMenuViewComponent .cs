using Ecommerce.Application.DTOs.Category;
using Ecommerce.Infrastructure.UnitOfWork;
using Ecommerce.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly UnitOfWork _uow;

    public CategoryMenuViewComponent(UnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _uow.Categories.Query()
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        var dtos = categories.Select(c => new CategoryVM
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ParentId = c.ParentId,
            DisplayOrder = c.DisplayOrder
        }).ToList();

        var parents = dtos.Where(c => c.ParentId == null).ToList();

        foreach (var parent in parents)
        {
            parent.Children = dtos
                .Where(c => c.ParentId == parent.Id)
                .OrderBy(c => c.DisplayOrder)
                .ToList();
        }

        return View(parents);
    }
}