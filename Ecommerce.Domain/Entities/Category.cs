
using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    // Navigation
    public Category? Parent { get; set; }  // ← nhiều Category con → 1 cha
    public ICollection<Category> Children { get; set; } = new List<Category>(); // 1 cha → N con
    public ICollection<Product> Products { get; set; } = new List<Product>(); // 1 category → N products
}
