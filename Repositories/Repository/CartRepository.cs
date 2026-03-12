using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repository
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly SalesAppDbContext _context;

        public CartRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CartId)
                .FirstOrDefaultAsync();
        }

        public async Task<Cart?> GetByIdAsync(int id)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CartId == id);
        }

        /// <summary>
        /// Get cart without entity tracking (for read-only operations after modifications)
        /// </summary>
        public async Task<Cart?> GetCartByUserIdAsNoTrackingAsync(int userId)
        {
            return await _context.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CartId)
                .FirstOrDefaultAsync();
        }
    }
}
