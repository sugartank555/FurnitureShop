using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ReportsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalRevenue = await _db.Orders.SumAsync(o => o.TotalAmount);
            ViewBag.TotalOrders = await _db.Orders.CountAsync();
            ViewBag.TopProducts = await _db.OrderItems
                .Include(i => i.Product)
                .GroupBy(i => i.Product.Name)
                .Select(g => new { Name = g.Key, Sold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Sold)
                .Take(5)
                .ToListAsync();
            return View();
        }
    }
}
