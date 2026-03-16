using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Inventory
{
    public class AdjustInventoryDto
    {
        public int Id { get; set; }
    
        public int AdjustmentQuantity { get; set; }
    
        public string Reason { get; set; } = string.Empty;
    
        public string AdjustmentType { get; set; } = "add";
    
        public string? Note { get; set; }
    }
}