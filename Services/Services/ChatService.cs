using AutoMapper;
using Repositories.Interfaces;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Services
{
    public interface IChatService
    {
        Task<ChatMessageResponseDTO> SendMessageAsync(ChatMessageRequestDTO request);
        Task<IEnumerable<ChatMessageResponseDTO>> GetConversationAsync(int userId1, int userId2);
        Task<IEnumerable<ChatConversationDTO>> GetUserConversationsAsync(int userId);
        Task<IEnumerable<ChatMessageResponseDTO>> GetUnreadMessagesAsync(int userId);
        Task<bool> MarkAsReadAsync(int messageId);
        Task<int> GetUnreadCountAsync(int userId);
    }

    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ChatMessageResponseDTO> SendMessageAsync(ChatMessageRequestDTO request)
        {
            // Validate sender exists
            var sender = await _unitOfWork.UserRepository.GetByIdAsync(request.SenderId);
            if (sender == null)
                throw new Exception("Sender not found");

            // Validate receiver exists
            var receiver = await _unitOfWork.UserRepository.GetByIdAsync(request.ReceiverId);
            if (receiver == null)
                throw new Exception("Receiver not found");

            var message = new ChatMessage
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                MessageText = request.Message,
                MessageType = request.MessageType,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.ChatMessageRepository.CreateAsync(message);
            await _unitOfWork.SaveChanges();

            return new ChatMessageResponseDTO
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                SenderName = sender.Username,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiver.Username,
                Message = message.MessageText,
                MessageType = message.MessageType,
                SentAt = message.SentAt ?? DateTime.UtcNow,
                IsRead = message.IsRead ?? false
            };
        }

        public async Task<IEnumerable<ChatMessageResponseDTO>> GetConversationAsync(int userId1, int userId2)
        {
            var messages = await _unitOfWork.ChatMessageRepository.GetConversationAsync(userId1, userId2);
            
            var result = new List<ChatMessageResponseDTO>();
            foreach (var msg in messages)
            {
                result.Add(new ChatMessageResponseDTO
                {
                    MessageId = msg.MessageId,
                    SenderId = msg.SenderId,
                    SenderName = msg.Sender?.Username ?? "Unknown",
                    ReceiverId = msg.ReceiverId,
                    ReceiverName = msg.Receiver?.Username ?? "Unknown",
                    Message = msg.MessageText,
                    MessageType = msg.MessageType,
                    SentAt = msg.SentAt ?? DateTime.UtcNow,
                    IsRead = msg.IsRead ?? false
                });
            }

            return result;
        }

        public async Task<IEnumerable<ChatConversationDTO>> GetUserConversationsAsync(int userId)
        {
            var messages = await _unitOfWork.ChatMessageRepository.GetUserConversationsAsync(userId);
            
            var conversations = new Dictionary<int, ChatConversationDTO>();

            foreach (var msg in messages)
            {
                var otherUserId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
                var otherUser = msg.SenderId == userId ? msg.Receiver : msg.Sender;

                if (!conversations.ContainsKey(otherUserId))
                {
                    conversations[otherUserId] = new ChatConversationDTO
                    {
                        ConversationId = otherUserId,
                        UserId = otherUserId,
                        UserName = otherUser?.Username ?? "Unknown",
                        LastMessage = msg.MessageText,
                        LastMessageTime = msg.SentAt ?? DateTime.UtcNow
                    };
                }
            }

            // Count unread messages for each conversation
            foreach (var convId in conversations.Keys.ToList())
            {
                var unreadCount = await GetUnreadCountForUserAsync(userId, convId);
                conversations[convId].UnreadCount = unreadCount;
            }

            return conversations.Values.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        public async Task<IEnumerable<ChatMessageResponseDTO>> GetUnreadMessagesAsync(int userId)
        {
            var messages = await _unitOfWork.ChatMessageRepository.GetUnreadMessagesAsync(userId);
            
            var result = new List<ChatMessageResponseDTO>();
            foreach (var msg in messages)
            {
                result.Add(new ChatMessageResponseDTO
                {
                    MessageId = msg.MessageId,
                    SenderId = msg.SenderId,
                    SenderName = msg.Sender?.Username ?? "Unknown",
                    ReceiverId = msg.ReceiverId,
                    ReceiverName = msg.Receiver?.Username ?? "Unknown",
                    Message = msg.MessageText,
                    MessageType = msg.MessageType,
                    SentAt = msg.SentAt ?? DateTime.UtcNow,
                    IsRead = false
                });
            }

            return result;
        }

        public async Task<bool> MarkAsReadAsync(int messageId)
        {
            var message = await _unitOfWork.ChatMessageRepository.GetByIdAsync(messageId);
            if (message == null) return false;

            message.IsRead = true;
            _unitOfWork.ChatMessageRepository.Update(message);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var messages = await _unitOfWork.ChatMessageRepository.GetUnreadMessagesAsync(userId);
            return messages.Count();
        }

        private async Task<int> GetUnreadCountForUserAsync(int userId, int otherUserId)
        {
            var messages = await _unitOfWork.ChatMessageRepository.GetConversationAsync(userId, otherUserId);
            return messages.Count(m => m.ReceiverId == userId && m.IsRead == false);
        }
    }
}
