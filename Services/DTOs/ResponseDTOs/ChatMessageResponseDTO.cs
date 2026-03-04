namespace Services.DTOs.ResponseDTOs
{
    public class ChatMessageResponseDTO
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = null!;
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string MessageType { get; set; } = "text";
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class ChatConversationDTO
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserImage { get; set; }
        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
