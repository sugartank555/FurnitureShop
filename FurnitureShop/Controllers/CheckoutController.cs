using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;
using FurnitureShop.Models;

namespace FurnitureShop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CheckoutController(ApplicationDbContext db) => _db = db;

        // 🧩 Lấy giỏ hàng của người dùng hiện tại
        private async Task<Cart> GetCart()
        {
            var userId = User.Claims.First(c => c.Type.Contains("nameidentifier")).Value;
            return await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? new Cart();
        }

        // 🛒 Trang hiển thị giỏ hàng & chọn phương thức thanh toán
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await GetCart();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            ViewBag.Total = cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0));
            return View(cart);
        }

        // ✅ Xác nhận đặt hàng (COD hoặc Momo)
        [HttpPost]
        public async Task<IActionResult> Confirm(string paymentMethod)
        {
            var cart = await GetCart();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("History", "Order");

            var order = new Order
            {
                UserId = cart.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0)),
                Status = OrderStatus.Pending,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product?.Price ?? 0
                }).ToList(),
                PaymentMethod = paymentMethod
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync();

            // Nếu chọn Momo → chuyển đến trang nhập thông tin giả lập
            if (paymentMethod == "Momo")
                return RedirectToAction("MomoPayment", new { orderId = order.Id });

            // Nếu chọn COD → hoàn tất đơn hàng luôn
            order.Status = OrderStatus.Completed;
            await _db.SaveChangesAsync();

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        // 💳 Trang nhập thông tin thanh toán Momo (giả lập)
        [HttpGet]
        public async Task<IActionResult> MomoPayment(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return RedirectToAction("Index", "Cart");

            ViewBag.OrderId = order.Id;
            ViewBag.Amount = order.TotalAmount;
            return View();
        }

        // 🧾 Xử lý khi người dùng nhấn "Xác nhận thanh toán"
        [HttpPost]
        public async Task<IActionResult> ProcessMomoPayment(int orderId, string momoNumber, string accountName)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return RedirectToAction("Index", "Cart");

            // Giả lập thanh toán thành công
            order.Status = OrderStatus.Completed;
            order.PaymentMethod = "Momo";
            await _db.SaveChangesAsync();

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        // ✅ Trang đặt hàng thành công
        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return View(order);
        }
    }
}
