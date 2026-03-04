using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<CartItem?> GetByIdAsync(int id);
        Task<IEnumerable<CartItem>> GetByCartIdAsync(int cartId);
        Task<CartItem?> GetByCartAndProductAsync(int cartId, int productId);
    }
}
