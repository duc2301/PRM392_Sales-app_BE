namespace Services.DTOs.ResponseDTOs
{
    public class NotificationResponseDTO
    {
        public int NotificationId { get; set; }

        public int? UserId { get; set; }

        public string? Message { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
