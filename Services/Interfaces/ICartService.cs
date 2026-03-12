using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDTO?> GetCartByUserIdAsync(int userId);
        Task<CartResponseDTO?> GetByIdAsync(int id);
        Task<CartResponseDTO> AddToCartAsync(int userId, CartItemRequestDTO request);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> RemoveCartItemAsync(int cartItemId);
        Task<CartResponseDTO?> ClearCartAsync(int userId);
        Task<CartBadgeResponseDTO> GetCartBadgeAsync(int userId);
    }
}
