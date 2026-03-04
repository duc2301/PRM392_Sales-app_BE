using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Services;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Create payment for order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var payment = await _paymentService.CreatePaymentAsync(request);
                return Created(nameof(CreatePayment), ApiResponse.Success("Payment created", payment));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Create payment failed", ex.Message));
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
                return BadRequest(ApiResponse.Fail("Get payment failed", ex.Message));
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
                return BadRequest(ApiResponse.Fail("Get payments failed", ex.Message));
            }
        }

        /// <summary>
        /// Confirm payment success
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                var payment = await _paymentService.ConfirmPaymentAsync(request.OrderId, request.Amount);
                return Ok(ApiResponse.Success("Payment confirmed successfully", payment));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Confirm payment failed", ex.Message));
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
                return BadRequest(ApiResponse.Fail("Update payment status failed", ex.Message));
            }
        }
    }
}
