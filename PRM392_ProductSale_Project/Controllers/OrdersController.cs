using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Services;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Create new order from cart
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var order = await _orderService.CreateOrderAsync(request);
                return Created(nameof(CreateOrder), ApiResponse.Success("Order created successfully", order));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Create order failed", ex.Message));
            }
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                    return NotFound(ApiResponse.Fail("Order not found"));

                return Ok(ApiResponse.Success("Get order success", order));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get order failed", ex.Message));
            }
        }

        /// <summary>
        /// Get all orders for user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(ApiResponse.Success("Get user orders success", orders));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get user orders failed", ex.Message));
            }
        }

        /// <summary>
        /// Cancel order (admin or user)
        /// </summary>
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(orderId);
                if (!result)
                    return NotFound(ApiResponse.Fail("Order not found"));

                return Ok(ApiResponse.Success("Order cancelled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Cancel order failed", ex.Message));
            }
        }

        /// <summary>
        /// Update order status (admin only)
        /// </summary>
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
                if (!result)
                    return NotFound(ApiResponse.Fail("Order not found"));

                return Ok(ApiResponse.Success("Order status updated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Update order status failed", ex.Message));
            }
        }
    }
}
