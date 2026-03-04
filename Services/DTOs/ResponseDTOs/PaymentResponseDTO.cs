namespace Services.DTOs.ResponseDTOs
{
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; } = null!; // pending, success, failed, cancelled
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionId { get; set; }
        public string? ResponseCode { get; set; }
    }
}
