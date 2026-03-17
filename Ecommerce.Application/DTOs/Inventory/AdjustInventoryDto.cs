using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Inventory
{
    // ADJUST
    public class AdjustInventoryDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Adjustment quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Adjustment quantity cannot be negative.")]
        public int AdjustmentQuantity { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [MaxLength(500, ErrorMessage = "Reason must be at most 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adjustment type is required.")]
        [RegularExpression("^(add|subtract|set)$",
            ErrorMessage = "Adjustment type must be 'add', 'subtract', or 'set'.")]
        public string AdjustmentType { get; set; } = "add";

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}