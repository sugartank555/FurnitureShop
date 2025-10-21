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

        private async Task<Cart> GetCart()
        {
            var userId = User.Claims.First(c => c.Type.Contains("nameidentifier")).Value;
            return await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product)
                                  .FirstOrDefaultAsync(c => c.UserId == userId) ?? new Cart();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await GetCart();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            ViewBag.Total = cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0));
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm()
        {
            var cart = await GetCart();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var userId = cart.UserId;
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Items.Sum(i => i.Quantity * (i.Product?.Price ?? 0)),
                Status = OrderStatus.Pending,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product?.Price ?? 0
                }).ToList()
            };

            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart.Items);
            await _db.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        public IActionResult Success() => View();
    }
}
