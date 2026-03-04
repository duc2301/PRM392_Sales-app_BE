using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.DbContexts;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Repository
{
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        private readonly SalesAppDbContext _context;

        public ChatMessageRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ChatMessage?> GetByIdAsync(int id)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == id);
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationAsync(int userId1, int userId2)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int receiverId)
        {
            return await _context.ChatMessages
                .Where(m => m.ReceiverId == receiverId && m.IsRead == false)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetUserConversationsAsync(int userId)
        {
            // Get latest message from each conversation
            return await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentAt)
                .DistinctBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .ToListAsync();
        }
    }
}
