using System.ComponentModel.DataAnnotations;
using Ecommerce.Application.Common;

namespace Ecommerce.Application.DTOs.Category;

public class CategoryDto
{
    public int      Id           { get; set; }
    public string   Name         { get; set; } = string.Empty;
    public string   Slug         { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public string?  ImageUrl     { get; set; }
    public int?     ParentId     { get; set; }
    public string?  ParentName   { get; set; }
    public bool     IsActive     { get; set; }
    public int      DisplayOrder { get; set; }
    public int      ProductCount { get; set; }
    public DateTime CreatedAt    { get; set; }
}

public class CategoryListDto
{
    public int      Id           { get; set; }
    public string   Name         { get; set; } = string.Empty;
    public string   Slug         { get; set; } = string.Empty;
    public bool     IsActive     { get; set; }
    public int?     ParentId     { get; set; }
    public string?  ParentName   { get; set; }
    public int      DisplayOrder { get; set; }
    public int      ProductCount { get; set; }
    public DateTime CreatedAt    { get; set; }
}

// FILTER
public class CategoryFilterParams : PaginationParams
{
    [MaxLength(100)]
    public string? Search { get; set; }
    public bool?   IsActive      { get; set; }
    public int?    ParentId      { get; set; }
    public string  SortBy        { get; set; } = "Name";
    public string  SortDirection { get; set; } = "asc";
}

// CREATE
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(120, ErrorMessage = "Slug must be at most 120 characters.")]
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase letters, numbers and hyphens only.")]
    public string? Slug { get; set; }

    [MaxLength(500, ErrorMessage = "Description must be at most 500 characters.")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Image URL must be a valid URL.")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999.")]
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateCategoryDto : CreateCategoryDto
{
    [Required]
    public int Id { get; set; }
}