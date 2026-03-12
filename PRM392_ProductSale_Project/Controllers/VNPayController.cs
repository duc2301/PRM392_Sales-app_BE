using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.Services;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayController : ControllerBase
    {
        private readonly IVNPayService _vnPayService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public VNPayController(IVNPayService vnPayService, IPaymentService paymentService, IOrderService orderService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
            _orderService = orderService;
            _configuration = configuration;
        }

        /// <summary>
        /// Debug endpoint - Test VNPay URL generation
        /// </summary>
        [HttpPost("debug/test-url")]
        public async Task<IActionResult> DebugTestUrl([FromBody] CreateVNPayPaymentRequest request)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var returnUrl = _configuration["VNPay:ReturnUrl"];

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.OrderId,
                    request.Amount,
                    $"Thanh toan don hang {request.OrderId}",
                    clientIp,
                    returnUrl
                );

                var uri = new Uri(paymentUrl);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

                return Ok(new
                {
                    success = true,
                    paymentUrl = paymentUrl,
                    debug = new
                    {
                        clientIp = clientIp,
                        returnUrl = returnUrl,
                        queryParams = queryParams.AllKeys.ToDictionary(k => k, k => queryParams[k]),
                        vnp_IpAddr = queryParams["vnp_IpAddr"],
                        vnp_Amount = queryParams["vnp_Amount"],
                        vnp_SecureHash = queryParams["vnp_SecureHash"]?.Substring(0, 20) + "...",
                        vnp_CreateDate = queryParams["vnp_CreateDate"],
                        vnp_TmnCode = queryParams["vnp_TmnCode"]
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Debug test failed", ex.Message));
            }
        }

        /// <summary>
        /// Create VNPay payment URL (redirect to VNPay)
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreateVNPayPaymentRequest request)
        {
            try
            {
                if (request.OrderId <= 0 || request.Amount <= 0)
                    return BadRequest(ApiResponse.Fail("Invalid order or amount"));

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var returnUrl = _configuration["VNPay:ReturnUrl"];

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.OrderId,
                    request.Amount,
                    $"Thanh toan don hang {request.OrderId}",
                    clientIp,
                    returnUrl
                );

                return Ok(ApiResponse.Success("Payment URL created", new { paymentUrl }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Create payment URL failed", ex.Message));
            }
        }

        /// <summary>
        /// VNPay payment return URL (called by VNPay after payment)
        /// Redirect from VNPay payment page back to our app
        /// </summary>
        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            IActionResult ReturnHtml(string title, string message, bool isSuccess)
            {
                var color = isSuccess ? "#4CAF50" : "#F44336";
                var icon = isSuccess ? "✅" : "❌";
                var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1'>
                <style>
                    body {{ display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f0f2f5; text-align: center; padding: 20px; }}
                    .card {{ background: white; padding: 40px 20px; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); max-width: 400px; width: 100%; }}
                    .icon {{ font-size: 50px; margin-bottom: 10px; }}
                    h2 {{ color: {color}; margin-top: 0; font-size: 24px; }}
                    p {{ color: #555; font-size: 16px; line-height: 1.5; margin-bottom: 20px; }}
                    .footer {{ font-size: 14px; color: #888; border-top: 1px solid #eee; padding-top: 15px; margin-top: 15px; }}
                </style>
            </head>
            <body>
                <div class='card'>
                    <div class='icon'>{icon}</div>
                    <h2>{title}</h2>
                    <p>{message}</p>
                    <div class='footer'>Bạn có thể đóng màn hình này để quay lại ứng dụng.</div>
                </div>
            </body>
            </html>";
                return Content(html, "text/html");
            }

            try
            {
                var responseData = new Dictionary<string, string>();
                foreach (var key in Request.Query.Keys)
                {
                    responseData[key] = Request.Query[key].ToString();
                }

                var paymentInfo = _vnPayService.ProcessPaymentReturn(responseData);

                if (paymentInfo.PaymentStatus == "success")
                {
                    if (paymentInfo.OrderId <= 0)
                        return ReturnHtml("Thanh toán thất bại", "Mã đơn hàng không hợp lệ.", false);

                    var amountStr = responseData.ContainsKey("vnp_Amount") ? responseData["vnp_Amount"] : "0";
                    if (!decimal.TryParse(amountStr, out var amount))
                        return ReturnHtml("Thanh toán thất bại", "Dữ liệu số tiền không hợp lệ.", false);

                    amount = amount / 100;

                    if (amount <= 0)
                        return ReturnHtml("Thanh toán thất bại", "Số tiền giao dịch không hợp lệ.", false);

                    var order = await _orderService.GetOrderByIdAsync(paymentInfo.OrderId);
                    if (order == null)
                        return ReturnHtml("Thanh toán thất bại", "Không tìm thấy đơn hàng trong hệ thống.", false);

                    var orderTotal = order.TotalAmount;
                    var tolerance = orderTotal * 0.01m;
                    if (Math.Abs(amount - orderTotal) > tolerance)
                        return ReturnHtml("Thanh toán thất bại", $"Số tiền thanh toán không khớp. Mong đợi: {orderTotal:N0}đ, Nhận được: {amount:N0}đ", false);

                    try
                    {
                        var payment = await _paymentService.ConfirmPaymentAsync(paymentInfo.OrderId, amount);
                        if (payment != null)
                        {
                            return ReturnHtml(
                                "Thanh toán thành công!",
                                $"Đơn hàng #{paymentInfo.OrderId} đã được ghi nhận.<br/>Mã giao dịch: <b>{payment.TransactionId}</b>",
                                true
                            );
                        }
                        else
                        {
                            return ReturnHtml("Thanh toán thất bại", "Không thể xác nhận giao dịch trên hệ thống.", false);
                        }
                    }
                    catch (Exception ex)
                    {
                        return ReturnHtml("Lỗi hệ thống", $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}", false);
                    }
                }
                else
                {
                    var responseCode = responseData.ContainsKey("vnp_ResponseCode") ? responseData["vnp_ResponseCode"] : "99";
                    return ReturnHtml("Giao dịch thất bại", $"Thanh toán bị hủy hoặc từ chối bởi ngân hàng.<br/>(Mã lỗi VNPay: {responseCode})", false);
                }
            }
            catch (Exception ex)
            {
                return ReturnHtml("Lỗi xử lý", $"Đã xảy ra lỗi không xác định: {ex.Message}", false);
            }
        }

        /// <summary>
        /// IPN Webhook from VNPay (Instant Payment Notification)
        /// VNPay sends this to confirm payment server-side, independent of user action
        /// This is a backup confirmation mechanism
        /// </summary>
        [HttpPost("ipn")]
        public async Task<IActionResult> IPN()
        {
            try
            {
                var responseData = new Dictionary<string, string>();
                foreach (var key in Request.Query.Keys)
                {
                    responseData[key] = Request.Query[key].ToString();
                }

                var paymentInfo = _vnPayService.ProcessPaymentReturn(responseData);

                if (paymentInfo.PaymentStatus == "success")
                {
                    try
                    {
                        var amountStr = responseData.ContainsKey("vnp_Amount") ? responseData["vnp_Amount"] : "0";
                        if (decimal.TryParse(amountStr, out var amount))
                        {
                            amount = amount / 100;

                            await _paymentService.ConfirmPaymentAsync(paymentInfo.OrderId, amount);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"IPN: Payment already processed or error: {ex.Message}");
                    }
                }

                return Ok(new { RspCode = "00", Message = "success" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPN Error: {ex.Message}");
                return Ok(new { RspCode = "99", Message = ex.Message });
            }
        }
    }

    public class CreateVNPayPaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}
