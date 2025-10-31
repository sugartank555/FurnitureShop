namespace FurnitureShop.Services
{
    public interface IPaymentService
    {
        Task<string> CreateMomoPayment(int orderId, decimal amount, string description);
        Task<bool> VerifyMomoCallback(Dictionary<string, string> callbackData);
    }
}