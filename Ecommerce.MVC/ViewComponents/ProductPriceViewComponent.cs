using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.MVC.ViewComponents
{
    public class ProductPriceViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(decimal basePrice, decimal? salePrice = null, string size = "md")
        {
            ViewBag.BasePrice  = basePrice;
            ViewBag.SalePrice  = salePrice;
            ViewBag.Size       = size;
            return View();
        }
    }
}