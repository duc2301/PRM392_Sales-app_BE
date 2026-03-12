using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Lấy cart của user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                if (cart == null)
                    return NotFound(ApiResponse.Fail("Cart not found"));

                return Ok(ApiResponse.Success("Get cart success", cart));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get cart failed", ex.Message));
            }
        }

        /// <summary>
        /// Thêm sản phẩm vào cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Cart item data</param>
        /// <returns></returns>
        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddToCart(int userId, [FromBody] CartItemRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var cart = await _cartService.AddToCartAsync(userId, request);
                return Ok(ApiResponse.Success("Add to cart success", cart));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Add to cart failed", ex.Message));
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cartItemId">Cart item ID</param>
        /// <param name="quantity">New quantity</param>
        /// <returns></returns>
        [HttpPut("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int userId, int cartItemId, [FromBody] int quantity)
        {
            try
            {
                if (quantity <= 0)
                    return BadRequest(ApiResponse.Fail("Invalid quantity"));

                var result = await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                if (!result)
                    return NotFound(ApiResponse.Fail("Cart item not found"));

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(ApiResponse.Success("Update cart item success", cart));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Update cart item failed", ex.Message));
            }
        }

        /// <summary>
        /// Xóa sản phẩm khỏi cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cartItemId">Cart item ID</param>
        /// <returns></returns>
        [HttpDelete("{userId}/items/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(int userId, int cartItemId)
        {
            try
            {
                var result = await _cartService.RemoveCartItemAsync(cartItemId);
                if (!result)
                    return NotFound(ApiResponse.Fail("Cart item not found"));

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(ApiResponse.Success("Remove cart item success", cart));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Remove cart item failed", ex.Message));
            }
        }

        /// <summary>
        /// Xóa toàn bộ sản phẩm trong cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpDelete("{userId}/clear")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                var result = await _cartService.ClearCartAsync(userId);
                if (result == null)
                    return NotFound(ApiResponse.Fail("Cart not found"));

                return Ok(ApiResponse.Success("Clear cart success", result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Clear cart failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy badge thông tin cart (số lượng items và tổng tiền)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("{userId}/badge")]
        public async Task<IActionResult> GetCartBadge(int userId)
        {
            try
            {
                var badge = await _cartService.GetCartBadgeAsync(userId);
                return Ok(ApiResponse.Success("Get cart badge success", badge));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get cart badge failed", ex.Message));
            }
        }
    }
}
