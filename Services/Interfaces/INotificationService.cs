using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationResponseDTO>> GetByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<CartBadgeResponseDTO> GetCartBadgeCountAsync(int userId);
    }
}
