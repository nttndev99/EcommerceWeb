using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Application.DTOs.Category;
using Ecommerce.Application.DTOs.Product;

namespace Ecommerce.MVC.Models
{
    public class HomeViewModel
    {
        public List<ProductListDto> Products { get; set; }
        public List<CategoryListDto> Categories { get; set; }
        public string ActiveTab { get; set; }
    }
    
}