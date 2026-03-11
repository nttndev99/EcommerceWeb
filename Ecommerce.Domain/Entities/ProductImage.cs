
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// Ảnh sản phẩm — có thể thuộc về Product (gallery chung) HOẶC ProductVariant (ảnh riêng)
///
/// RELATIONSHIPS:
///   Product        (1) ────< ProductImage (N)   [ProductId FK — luôn có]
///   ProductVariant (1) ────< ProductImage (N)   [VariantId  FK — nullable]
///
/// Khi VariantId != null → ảnh này hiển thị khi chọn variant đó
/// Khi VariantId == null → ảnh dùng chung cho toàn sản phẩm
/// </summary>
public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Product Product { get; set; } = null!;
}
