
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.MVC.Areas.Admin.Models
{
    // ===== ProductImages =====
    public class ProductImagesViewModel
    {
        public ProductDto  Product { get; set; } = new();
        public List<ProductImageDto> Images  { get; set; } = new();
    }

}