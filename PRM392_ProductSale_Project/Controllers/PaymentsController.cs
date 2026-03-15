using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Services;
using Microsoft.Extensions.Configuration;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVNPayService _vnPayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService, 
            IVNPayService vnPayService, 
            IConfiguration configuration,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _vnPayService = vnPayService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Create payment for order (Pending status)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var payment = await _paymentService.CreatePaymentAsync(request);
                return Created(nameof(CreatePayment), ApiResponse.Success("Payment created successfully", payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create payment failed");
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
            
        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPayment(int paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                    return NotFound(ApiResponse.Fail("Payment not found"));

                return Ok(ApiResponse.Success("Get payment success", payment));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get payment {paymentId} failed");
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Get payments for order
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderPayments(int orderId)
        {
            try
            {
                var payments = await _paymentService.GetOrderPaymentsAsync(orderId);
                return Ok(ApiResponse.Success("Get payments success", payments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get order payments {orderId} failed");
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Update payment status (admin)
        /// </summary>
        [HttpPut("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int paymentId, [FromBody] string status)
        {
            try
            {
                var result = await _paymentService.UpdatePaymentStatusAsync(paymentId, status);
                if (!result)
                    return NotFound(ApiResponse.Fail("Payment not found"));

                return Ok(ApiResponse.Success("Payment status updated"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update payment {paymentId} status failed");
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Create VNPay payment URL for an existing pending payment
        /// </summary>
        [HttpPost("vnpay/create-url")]
        public async Task<IActionResult> CreateVNPayUrl([FromBody] CreateVNPayUrlRequest request)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(request.PaymentId);
                if (payment == null)
                    return NotFound(ApiResponse.Fail("Payment not found"));

                if (payment.PaymentStatus == "success")
                    return BadRequest(ApiResponse.Fail("Payment already verified successful"));

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                // Fixed ReturnUrl to point to this controller's action
                var returnUrl = _configuration["VNPay:ReturnUrl"] ?? $"{Request.Scheme}://{Request.Host}/api/Payments/vnpay-return";

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    payment.PaymentId,
                    payment.Amount,
                    $"Thanh toan don hang {payment.OrderId}",
                    clientIp,
                    returnUrl
                );

                return Ok(ApiResponse.Success("Payment URL created", new { paymentUrl }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create VNPay URL failed");
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// VNPay return URL handler (redirects user after payment)
        /// </summary>
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn()
        {
            try
            {
                var paymentInfo = _vnPayService.ProcessPaymentReturn(Request.Query);

                // lấy PaymentId từ vnp_TxnRef
                var paymentId = int.Parse(Request.Query["vnp_TxnRef"]);

                if (paymentInfo.PaymentStatus == "success")
                {
                    var payment = await _paymentService.ConfirmPaymentAsync(
                        paymentId,
                        Request.Query["vnp_TransactionNo"],
                        "VNPay",
                        "VNPay"
                    );

                    return Content($"Payment {paymentId} success", "text/plain");
                }
                else
                {
                    await _paymentService.FailPaymentAsync(
                        paymentId,
                        Request.Query["vnp_ResponseCode"]
                    );

                    return Content($"Payment {paymentId} failed", "text/plain");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VNPay Return error");
                return Content("Payment processing error", "text/plain");
            }
        }

        /// <summary>
        /// IPN Webhook from VNPay (Server-to-Server)
        /// </summary>
        [HttpGet("vnpay/ipn")]
        public async Task<IActionResult> VNPayIpn()
        {
            try
            {
                var responseData = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
                var paymentInfo = _vnPayService.ProcessPaymentReturn(Request.Query);
                var paymentId = paymentInfo.OrderId; // Actually PaymentId

                if (paymentInfo.PaymentStatus == "success")
                {
                    await _paymentService.ConfirmPaymentAsync(
                        paymentId, 
                        paymentInfo.TransactionId ?? "", 
                        "VNPay", 
                        "VNPay");
                }
                else
                {
                    await _paymentService.FailPaymentAsync(paymentId, paymentInfo.ResponseCode ?? "99");
                }

                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IPN handling failed");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }
    }

    public class CreateVNPayUrlRequest
    {
        public int PaymentId { get; set; }
    }
}
