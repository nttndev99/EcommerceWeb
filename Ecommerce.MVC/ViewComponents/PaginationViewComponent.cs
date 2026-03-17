using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.MVC.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            int currentPage,
            int totalPages,
            Dictionary<string, object>? routeValues = null)
        {
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages  = totalPages;
            ViewBag.RouteValues = routeValues ?? new();
            return View();
        }
    }
}