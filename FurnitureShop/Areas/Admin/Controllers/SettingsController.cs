using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveSettings(string siteName, string themeColor)
        {
            // Giả lập lưu cài đặt, bạn có thể thay bằng DB hoặc config thật
            ViewBag.Message = "Cài đặt đã được lưu thành công!";
            return View("Index");
        }
    }
}
