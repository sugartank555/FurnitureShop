using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace FurnitureShop.Services
{
    // Fake Email Sender: dùng cho môi trường phát triển, không gửi thật
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[FakeEmailSender] To: {email}, Subject: {subject}");
            Console.WriteLine(htmlMessage);
            return Task.CompletedTask;
        }
    }
}
