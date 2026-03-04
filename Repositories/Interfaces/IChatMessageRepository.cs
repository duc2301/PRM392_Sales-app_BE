using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<ChatMessage?> GetByIdAsync(int id);
        Task<IEnumerable<ChatMessage>> GetConversationAsync(int userId1, int userId2);
        Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int receiverId);
        Task<IEnumerable<ChatMessage>> GetUserConversationsAsync(int userId);
    }
}
