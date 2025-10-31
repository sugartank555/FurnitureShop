using FurnitureShop.Data;
using FurnitureShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db) => _db = db;

        // Lấy giỏ hàng của user hoặc tạo mới nếu chưa có
        private async Task<Cart> GetOrCreateCart()
        {
            var userId = User.Claims.First(c => c.Type.Contains("nameidentifier")).Value;
            var cart = await _db.Carts.Include(c => c.Items!)
                                    .ThenInclude(i => i.Product)
                                    .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }
            return cart;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await GetOrCreateCart();
            var item = cart.Items!.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
                item.Quantity += quantity;
            else
                cart.Items!.Add(new CartItem { ProductId = productId, Quantity = quantity });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var cart = await GetOrCreateCart();
            var item = cart.Items!.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var cart = await GetOrCreateCart();
            _db.CartItems.RemoveRange(cart.Items!);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = User?.Claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
            if (userId != null)
            {
                var count = _db.CartItems.Count(i => i.Cart!.UserId == userId);
                ViewBag.CartCount = count;
            }
            base.OnActionExecuting(context);
        }

    }
}
