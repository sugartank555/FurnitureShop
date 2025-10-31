using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace FurnitureShop.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public PaymentService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> CreateMomoPayment(int orderId, decimal amount, string description)
        {
            var partnerCode = _config["Momo:PartnerCode"];
            var accessKey = _config["Momo:AccessKey"];
            var secretKey = _config["Momo:SecretKey"];
            var endpoint = _config["Momo:Endpoint"];
            var redirectUrl = _config["Momo:RedirectUrl"];
            var ipnUrl = _config["Momo:IpnUrl"];

            var requestId = Guid.NewGuid().ToString();
            var orderId_str = "MM" + DateTime.Now.Ticks;
            var orderInfo = description;
            var requestType = "captureWallet";
            var extraData = "";

            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId_str}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSHA256(rawHash, secretKey);

            var requestData = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount = (long)amount,
                orderId = orderId_str,
                orderInfo,
                redirectUrl,
                ipnUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var result = await response.Content.ReadAsStringAsync();
            var resultJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);

            return resultJson?["payUrl"]?.ToString() ?? "";
        }

        public async Task<bool> VerifyMomoCallback(Dictionary<string, string> callbackData)
        {
            var secretKey = _config["Momo:SecretKey"];
            var signature = callbackData["signature"];

            var rawHash = $"accessKey={callbackData["accessKey"]}&amount={callbackData["amount"]}&extraData={callbackData["extraData"]}&message={callbackData["message"]}&orderId={callbackData["orderId"]}&orderInfo={callbackData["orderInfo"]}&orderType={callbackData["orderType"]}&partnerCode={callbackData["partnerCode"]}&payType={callbackData["payType"]}&requestId={callbackData["requestId"]}&responseTime={callbackData["responseTime"]}&resultCode={callbackData["resultCode"]}&transId={callbackData["transId"]}";

            var checksum = HmacSHA256(rawHash, secretKey);
            return signature == checksum;
        }

        private string HmacSHA256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}