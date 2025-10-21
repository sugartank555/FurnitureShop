using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FurnitureShop.Data;
using FurnitureShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() =>
            View(await _db.Categories.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Category c)
        {
            if (ModelState.IsValid)
            {
                _db.Add(c);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(c);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category c)
        {
            if (id != c.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _db.Update(c);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(c);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c != null)
            {
                _db.Categories.Remove(c);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
