using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.DTOs.ResponseDTOs;
using Services.Utils;
using VNPay.NetCore;

namespace Services.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int paymentId, decimal amount, string orderDescription, string clientIp, string returnUrl);
        VNPayReturnDTO ProcessPaymentReturn(IQueryCollection query);
    }

    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(int paymentId, decimal amount, string orderDescription,
    string clientIp, string returnUrl)
        {
            var vnp_TmnCode = _configuration["VNPay:TmnCode"]?.Trim();
            var vnp_HashSecret = _configuration["VNPay:HashSecret"]?.Trim();
            var vnp_BaseUrl = _configuration["VNPay:BaseUrl"]?.Trim().TrimEnd('/');

            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
                throw new Exception("VNPay configuration is missing");

            if (clientIp == "::1" || string.IsNullOrEmpty(clientIp))
                clientIp = "127.0.0.1";

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", vnTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", clientIp);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderDescription);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", paymentId.ToString());

            // thêm expire để chuẩn VNPay
            vnpay.AddRequestData("vnp_ExpireDate", vnTime.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            return vnpay.CreateRequestUrl(vnp_BaseUrl, vnp_HashSecret);
        }

        public VNPayReturnDTO ProcessPaymentReturn(IQueryCollection query)
        {
            var hashSecret = _configuration["VNPay:HashSecret"]?.Trim();
            var vnpay = new VnPayLibrary();

            foreach (var key in query.Keys)
            {
                if (key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    vnpay.AddResponseData(key, query[key]);
                }
            }

            var secureHash = query["vnp_SecureHash"];

            if (!vnpay.ValidateSignature(secureHash, hashSecret))
                throw new Exception("Invalid signature");

            var responseCode = query["vnp_ResponseCode"];
            var transactionStatus = query["vnp_TransactionStatus"];
            var paymentId = int.Parse(query["vnp_TxnRef"]);

            var paymentStatus = (responseCode == "00" && transactionStatus == "00")
                ? "success"
                : "failed";

            return new VNPayReturnDTO
            {
                OrderId = paymentId,
                PaymentStatus = paymentStatus,
                ResponseCode = responseCode,
                TransactionId = query["vnp_TransactionNo"]
            };
        }
    }
}