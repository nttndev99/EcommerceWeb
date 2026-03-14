using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int    OrderId          { get; set; }
        public int    ProductId        { get; set; }
        public int?   ProductVariantId { get; set; }

        // Snapshot tại thời điểm đặt hàng
        public string  ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public string? SKU         { get; set; }
        public string? ImageUrl    { get; set; }

        public int     Quantity   { get; set; }
        public decimal UnitPrice  { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        // Navigation
        public Order           Order          { get; set; } = null!;
        public Product         Product        { get; set; } = null!;
        public ProductVariant? ProductVariant { get; set; }
    }
}