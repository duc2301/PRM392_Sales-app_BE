namespace Services.DTOs.RequestDTOs
{
    public class ChatMessageRequestDTO
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; } = null!;
        public string MessageType { get; set; } = "text"; // text, image, file
    }
}
