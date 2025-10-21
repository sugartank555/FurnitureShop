using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FurnitureShop.Data;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        public IActionResult Index()
        {
            ViewBag.TotalProducts = _db.Products.Count();
            ViewBag.TotalOrders = _db.Orders.Count();
            ViewBag.TotalUsers = _db.Users.Count();
            return View();
        }
    }
}
