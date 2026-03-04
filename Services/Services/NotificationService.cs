using AutoMapper;
using Repositories.Interfaces;
using Services.DTOs.ResponseDTOs;
using Services.Interfaces;

namespace Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationResponseDTO>> GetByUserIdAsync(int userId)
        {
            var notifications = await _unitOfWork.NotificationRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponseDTO>>(notifications);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.NotificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<CartBadgeResponseDTO> GetCartBadgeCountAsync(int userId)
        {
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new CartBadgeResponseDTO { CartItemCount = 0, TotalPrice = 0 };
            }

            return new CartBadgeResponseDTO
            {
                CartItemCount = cart.CartItems.Sum(ci => ci.Quantity),
                TotalPrice = cart.TotalPrice
            };
        }
    }
}
