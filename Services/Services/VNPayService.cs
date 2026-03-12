using Microsoft.Extensions.Configuration;
using Services.DTOs.ResponseDTOs;
using Services.Utils;

namespace Services.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string orderDescription, string clientIp, string returnUrl);
        PaymentResponseDTO ProcessPaymentReturn(Dictionary<string, string> responseData);
    }

    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, string orderDescription,
            string clientIp, string returnUrl)
        {
            var tmnCode = _configuration["VNPay:TmnCode"]?.Trim();
            var hashSecret = _configuration["VNPay:HashSecret"]?.Trim();

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
                throw new Exception("VNPay configuration is missing");

            if (clientIp == "::1" || string.IsNullOrEmpty(clientIp))
                clientIp = "127.0.0.1";

            var sanitizedDescription = System.Text.RegularExpressions.Regex.Replace(orderDescription, @"[^\w\s\-\.@]", "");

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var vnpay = new VNPayHelper();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", vnTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", clientIp);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", sanitizedDescription);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

            var baseUrl = _configuration["VNPay:BaseUrl"];
            return vnpay.CreateRequestUrl(baseUrl, hashSecret);
        }

        public PaymentResponseDTO ProcessPaymentReturn(Dictionary<string, string> responseData)
        {
            var hashSecret = _configuration["VNPay:HashSecret"]?.Trim();
            var vnpay = new VNPayHelper();

            foreach (var kv in responseData)
            {
                if (!string.IsNullOrEmpty(kv.Key) && kv.Key.StartsWith("vnp_"))
                {
                    if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                    {
                        vnpay.AddResponseData(kv.Key, kv.Value);
                    }
                }
            }

            var vnp_SecureHash = responseData.ContainsKey("vnp_SecureHash") ? responseData["vnp_SecureHash"] : "";

            if (!vnpay.ValidateSignature(vnp_SecureHash, hashSecret))
                throw new Exception("Invalid signature");

            var responseCode = responseData.ContainsKey("vnp_ResponseCode") ? responseData["vnp_ResponseCode"] : "";
            var transactionStatus = responseData.ContainsKey("vnp_TransactionStatus") ? responseData["vnp_TransactionStatus"] : "";
            var orderId = int.Parse(responseData.ContainsKey("vnp_TxnRef") ? responseData["vnp_TxnRef"] : "0");

            var paymentStatus = (responseCode == "00" && transactionStatus == "00") ? "success" : "failed";

            return new PaymentResponseDTO
            {
                OrderId = orderId,
                PaymentStatus = paymentStatus,
                ResponseCode = responseCode,
                TransactionId = responseData.ContainsKey("vnp_TransactionNo") ? responseData["vnp_TransactionNo"] : ""
            };
        }
    }
}