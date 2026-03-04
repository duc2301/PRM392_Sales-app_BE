using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Cart?> GetByIdAsync(int id);
    }
}
