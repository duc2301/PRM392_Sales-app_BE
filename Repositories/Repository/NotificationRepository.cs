using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly SalesAppDbContext _context;

        public NotificationRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false)
                .CountAsync();
        }
    }
}
