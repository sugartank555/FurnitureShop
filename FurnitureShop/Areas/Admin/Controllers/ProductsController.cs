using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Data;
using FurnitureShop.Models;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var list = await _db.Products.Include(p => p.Category).ToListAsync();
            return View(list);
        }

        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await imageFile.CopyToAsync(stream);
                    product.ImageUrl = "/uploads/" + fileName;
                }

                _db.Add(product);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string folder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await imageFile.CopyToAsync(stream);
                    product.ImageUrl = "/uploads/" + fileName;
                }

                _db.Update(product);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Category)
                                            .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
