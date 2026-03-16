using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.DTOs.Order
{
    public class OrderStatDto
    {
        public string  UserId      { get; set; } = string.Empty;
        public int     TotalOrders { get; set; }
        public decimal TotalSpent  { get; set; }
    }
}