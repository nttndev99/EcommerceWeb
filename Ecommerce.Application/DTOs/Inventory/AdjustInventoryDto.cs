using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Inventory
{
    public class AdjustInventoryDto
    {
        public int Id { get; set; }
    
        /// <summary>
        /// Số lượng điều chỉnh: dương = nhập kho, âm = xuất kho
        /// </summary>
        public int AdjustmentQuantity { get; set; }
    
        /// <summary>
        /// Lý do điều chỉnh tồn kho
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    
        /// <summary>
        /// Loại điều chỉnh: "add" | "subtract" | "set"
        /// </summary>
        public string AdjustmentType { get; set; } = "add";
    
        public string? Note { get; set; }
    }
}