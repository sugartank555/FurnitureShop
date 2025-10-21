using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;

namespace FurnitureShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewBag.Featured = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();
            return View();
        }
        public async Task<IActionResult> Shop(int? categoryId, string? priceRange, string? brand, string? search)
        {
            var products = _db.Products.Include(p => p.Category).AsQueryable();

            // 🔍 Lọc theo tên sản phẩm
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            // Lọc theo danh mục
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            // Lọc theo giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                products = priceRange switch
                {
                    "low" => products.Where(p => p.Price < 1_000_000),
                    "mid" => products.Where(p => p.Price >= 1_000_000 && p.Price <= 5_000_000),
                    "high" => products.Where(p => p.Price > 5_000_000),
                    _ => products
                };
            }

            // Lọc theo thương hiệu (Brand)
            if (!string.IsNullOrEmpty(brand))
                products = products.Where(p => p.Brand.Contains(brand));

            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.PriceRange = priceRange;
            ViewBag.Brand = brand;
            ViewBag.CategoryId = categoryId;

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products.Include(p => p.Category)
                                            .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
