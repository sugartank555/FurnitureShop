using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FurnitureShop.Models;

namespace FurnitureShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Danh sách người dùng
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var list = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = string.Join(", ", roles)
                });
            }

            return View(list);
        }

        // Toggle quyền Admin
        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "Admin");

            return RedirectToAction(nameof(Index));
        }

        // Khóa / Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now)
            {
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100); // khóa dài
            }
            else
            {
                user.LockoutEnd = null; // mở khóa
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }

    // ViewModel đơn giản để hiển thị
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Roles { get; set; }
    }
}
