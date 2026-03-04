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

        public VNPayController(IVNPayService vnPayService, IPaymentService paymentService)
        {
            _vnPayService = vnPayService;
            _paymentService = paymentService;
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
                var returnUrl = $"{Request.Scheme}://{Request.Host}/api/vnpay/payment-return";

                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    request.OrderId,
                    request.Amount,
                    $"Thanh toán đơn hàng #{request.OrderId}",
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
        /// </summary>
        [HttpGet("payment-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            try
            {
                // Get response from VNPay
                var responseData = new Dictionary<string, string>();
                foreach (var key in Request.Query.Keys)
                {
                    responseData[key] = Request.Query[key].ToString();
                }

                // Process payment
                var paymentInfo = _vnPayService.ProcessPaymentReturn(responseData);

                if (paymentInfo.PaymentStatus == "success")
                {
                    // Update payment status
                    await _paymentService.ConfirmPaymentAsync(paymentInfo.OrderId, paymentInfo.Amount);

                    return Ok(ApiResponse.Success("Payment successful", paymentInfo));
                }
                else
                {
                    return BadRequest(ApiResponse.Fail("Payment failed", "Transaction not successful"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Payment return processing failed", ex.Message));
            }
        }

        /// <summary>
        /// Check payment status (webhook from VNPay)
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
                
                // Log IPN and update database
                if (paymentInfo.PaymentStatus == "success")
                {
                    await _paymentService.ConfirmPaymentAsync(paymentInfo.OrderId, paymentInfo.Amount);
                }

                return Ok(new { RspCode = "00", Message = "success" });
            }
            catch (Exception ex)
            {
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
