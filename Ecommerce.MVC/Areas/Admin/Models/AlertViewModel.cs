
namespace Ecommerce.MVC.Areas.Admin.Models
{
    public class AlertViewModel
    {
        public string Type { get; set; } = "success"; // success, danger, warning, info
        public string Message { get; set; } = string.Empty;
    }

}