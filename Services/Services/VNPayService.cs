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
        private readonly IPaymentService _paymentService;

        public VNPayService(IConfiguration configuration, IPaymentService paymentService)
        {
            _configuration = configuration;
            _paymentService = paymentService;
        }

        public string CreatePaymentUrl(int orderId, decimal amount, string orderDescription, 
            string clientIp, string returnUrl)
        {
            var tmnCode = _configuration["VNPay:TmnCode"];
            var hashSecret = _configuration["VNPay:HashSecret"];

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret))
                throw new Exception("VNPay configuration is missing");

            // Create payment request data
            var paymentData = VNPayHelper.CreatePaymentRequest(
                tmnCode,
                hashSecret,
                returnUrl,
                orderId,
                (long)amount,
                orderDescription,
                clientIp
            );

            // Build payment URL
            var paymentUrl = VNPayConfig.VnPayUrl + "?";
            foreach (var kv in paymentData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    paymentUrl += $"{kv.Key}={Uri.EscapeUriString(kv.Value)}&";
                }
            }

            return paymentUrl.TrimEnd('&');
        }

        public PaymentResponseDTO ProcessPaymentReturn(Dictionary<string, string> responseData)
        {
            var hashSecret = _configuration["VNPay:HashSecret"];

            // Validate signature
            if (!VNPayHelper.ValidateSignature(responseData, hashSecret))
                throw new Exception("Invalid signature");

            var responseCode = responseData.ContainsKey("vnp_ResponseCode") 
                ? responseData["vnp_ResponseCode"] 
                : "";
            var transactionStatus = responseData.ContainsKey("vnp_TransactionStatus") 
                ? responseData["vnp_TransactionStatus"] 
                : "";
            var orderId = int.Parse(responseData.ContainsKey("vnp_TxnRef") 
                ? responseData["vnp_TxnRef"] 
                : "0");

            var paymentStatus = (responseCode == "00" && transactionStatus == "00") ? "success" : "failed";

            var paymentDto = new PaymentResponseDTO
            {
                OrderId = orderId,
                PaymentStatus = paymentStatus,
                ResponseCode = responseCode,
                TransactionId = responseData.ContainsKey("vnp_TransactionNo") 
                    ? responseData["vnp_TransactionNo"] 
                    : ""
            };

            return paymentDto;
        }
    }
}

