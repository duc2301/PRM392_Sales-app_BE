using Microsoft.AspNetCore.Mvc;
using PRM392_ProductSale_Project.ApiResponseDTO;
using Services.DTOs.RequestDTOs;
using Services.Services;

namespace PRM392_ProductSale_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Send chat message
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse.Fail("Invalid data", ModelState));

                var message = await _chatService.SendMessageAsync(request);
                return Ok(ApiResponse.Success("Message sent successfully", message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Send message failed", ex.Message));
            }
        }

        /// <summary>
        /// Get conversation between two users
        /// </summary>
        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation([FromQuery] int userId1, [FromQuery] int userId2)
        {
            try
            {
                var messages = await _chatService.GetConversationAsync(userId1, userId2);
                return Ok(ApiResponse.Success("Get conversation success", messages));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get conversation failed", ex.Message));
            }
        }

        /// <summary>
        /// Get all conversations for user
        /// </summary>
        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetUserConversations(int userId)
        {
            try
            {
                var conversations = await _chatService.GetUserConversationsAsync(userId);
                return Ok(ApiResponse.Success("Get conversations success", conversations));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get conversations failed", ex.Message));
            }
        }

        /// <summary>
        /// Get unread messages for user
        /// </summary>
        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadMessages(int userId)
        {
            try
            {
                var messages = await _chatService.GetUnreadMessagesAsync(userId);
                return Ok(ApiResponse.Success("Get unread messages success", messages));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get unread messages failed", ex.Message));
            }
        }

        /// <summary>
        /// Get unread message count for user
        /// </summary>
        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            try
            {
                var count = await _chatService.GetUnreadCountAsync(userId);
                return Ok(ApiResponse.Success("Get unread count success", new { unreadCount = count }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Get unread count failed", ex.Message));
            }
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            try
            {
                var result = await _chatService.MarkAsReadAsync(messageId);
                if (!result)
                    return NotFound(ApiResponse.Fail("Message not found"));

                return Ok(ApiResponse.Success("Message marked as read"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail("Mark as read failed", ex.Message));
            }
        }
    }
}
