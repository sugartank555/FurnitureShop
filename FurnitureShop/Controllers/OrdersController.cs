using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;
using FurnitureShop.Models;

namespace FurnitureShop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db) => _db = db;

        // 🧾 Hiển thị lịch sử đơn hàng của người dùng hiện tại
        public async Task<IActionResult> History()
        {
            var userId = User.Claims.First(c => c.Type.Contains("nameidentifier")).Value;
            var orders = await _db.Orders
                .Include(o => o.Items!)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 🔍 Xem chi tiết 1 đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.Claims.First(c => c.Type.Contains("nameidentifier")).Value;
            var order = await _db.Orders
                .Include(o => o.Items!)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
