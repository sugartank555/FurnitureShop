using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;
using FurnitureShop.Models;
using Microsoft.AspNetCore.Authorization;
using FurnitureShop.Services;

namespace FurnitureShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public HomeController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        // ----------------- TRANG CHỦ -----------------
        public async Task<IActionResult> Index()
        {
            ViewBag.Featured = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(6)
                .ToListAsync();
            return View();
        }

        // ----------------- TRANG SHOP -----------------
        public async Task<IActionResult> Shop(int? categoryId, string? priceRange, string? brand, string? search)
        {
            var products = _db.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo tên sản phẩm
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
                    "low" => products.Where(p => p.Price < 5_000_000),
                    "mid" => products.Where(p => p.Price >= 5_000_000 && p.Price <= 10_000_000),
                    "high" => products.Where(p => p.Price > 10_000_000),
                    _ => products
                };
            }

            // Lọc theo thương hiệu
            if (!string.IsNullOrEmpty(brand))
                products = products.Where(p => p.Brand.Contains(brand));

            // Gửi dữ liệu ra View
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.PriceRange = priceRange;
            ViewBag.Brand = brand;
            ViewBag.CategoryId = categoryId;

            return View(await products.ToListAsync());
        }

        // ----------------- TRANG CHI TIẾT -----------------
        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ----------------- GIỚI THIỆU -----------------
        public IActionResult About()
        {
            ViewData["Title"] = "Giới thiệu về FurnitureShop";
            return View();
        }

        // ----------------- DỊCH VỤ -----------------
        public IActionResult Services()
        {
            ViewData["Title"] = "Dịch vụ của FurnitureShop";
            return View();
        }

        // ----------------- TRANG LIÊN HỆ (GET) -----------------}

            [HttpGet]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ với chúng tôi";
            return View();
        }

        // 🧩 Không cần xử lý POST thật — chỉ mô phỏng phản hồi
        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // Không làm gì cả — chỉ để form tượng trưng có thể submit
            ViewBag.Success = "Cảm ơn bạn đã gửi thông tin! (Chức năng demo)";
            return View();
        }
    }
}
