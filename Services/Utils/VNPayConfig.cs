namespace Services.Utils
{
    public class VNPayConfig
    {
        public const string VnPayUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public const string VnPayQueryUrl = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
        public const string VnPayRefundUrl = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction/refund";
        
        // These should be from appsettings.json
        public string TmnCode { get; set; } = null!;
        public string HashSecret { get; set; } = null!;
        public string ReturnUrl { get; set; } = null!;
        public string NotifyUrl { get; set; } = null!;
    }
}
