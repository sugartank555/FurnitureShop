using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FurnitureShop.Services;
using FurnitureShop.Data;
using Microsoft.EntityFrameworkCore;
using FurnitureShop.Models;
namespace FurnitureShop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _db;

        public PaymentController(IPaymentService paymentService, ApplicationDbContext db)
        {
            _paymentService = paymentService;
            _db = db;
        }

        // ============ MOMO CALLBACK ============
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MomoCallback()
        {
            try
            {
                var callbackData = new Dictionary<string, string>();
                foreach (var key in Request.Form.Keys)
                {
                    callbackData[key] = Request.Form[key];
                }

                var isValid = await _paymentService.VerifyMomoCallback(callbackData);

                if (isValid && callbackData["resultCode"] == "0")
                {
                    // Cập nhật trạng thái đơn hàng
                    var orderId = ExtractOrderIdFromMomo(callbackData["orderId"]);
                    var order = await _db.Orders.FindAsync(orderId);

                    if (order != null)
                    {
                        order.Status = OrderStatus.Completed;
                        order.PaymentMethod = "Momo";
                        await _db.SaveChangesAsync();
                    }
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        // ============ MOMO RETURN ============
        [HttpGet]
        public IActionResult MomoReturn(string resultCode, string orderId)
        {
            if (resultCode == "0")
            {
                TempData["Success"] = "Thanh toán Momo thành công!";
                
                // Extract order ID và chuyển đến trang thành công
                var realOrderId = ExtractOrderIdFromMomo(orderId);
                return RedirectToAction("OrderSuccess", "Checkout", new { orderId = realOrderId });
            }
            else
            {
                TempData["Error"] = "Thanh toán Momo thất bại!";
                return RedirectToAction("Index", "Cart");
            }
        }

        // ============ HELPER ============
        private int ExtractOrderIdFromMomo(string momoOrderId)
        {
            // Format: MMOrderId (remove MM prefix)
            return int.Parse(momoOrderId.Substring(2));
        }
    }
}