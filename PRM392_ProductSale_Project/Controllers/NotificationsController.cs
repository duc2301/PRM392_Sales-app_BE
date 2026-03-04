using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.Interfaces;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Lấy badge số lượng items trong cart (cho app icon notification)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Cart badge count and total price</returns>
        [HttpGet("{userId}/badge-count")]
        public async Task<IActionResult> GetCartBadgeCount(int userId)
        {
            try
            {
                var badge = await _notificationService.GetCartBadgeCountAsync(userId);
                return Ok(ApiResponse.Success("Get cart badge count success", badge));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get cart badge count failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy tất cả notifications của user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetByUserIdAsync(userId);
                return Ok(ApiResponse.Success("Get notifications success", notifications));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get notifications failed", ex.Message));
            }
        }

        /// <summary>
        /// Lấy số lượng notifications chưa đọc
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(ApiResponse.Success("Get unread count success", count));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get unread count failed", ex.Message));
            }
        }
    }
}
