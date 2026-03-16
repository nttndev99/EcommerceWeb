using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Product
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRODUCT IMAGE DTOs
    // ═══════════════════════════════════════════════════════════════════════════

    public class ProductImageDto
    {
        public int     Id           { get; set; }
        public int     ProductId    { get; set; }
        public string  ImageUrl     { get; set; } = string.Empty;
        public string? AltText      { get; set; }
        public bool    IsPrimary    { get; set; }
        public int     DisplayOrder { get; set; }
    }

    public class CreateProductImageDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Alt text must be at most 200 characters.")]
        public string? AltText { get; set; }

        public bool IsPrimary    { get; set; } = false;

        [Range(0, 9999)]
        public int DisplayOrder  { get; set; } = 0;
    }

    public class UpdateProductImageDto : CreateProductImageDto
    {
        [Required]
        public int Id { get; set; }
    }
}