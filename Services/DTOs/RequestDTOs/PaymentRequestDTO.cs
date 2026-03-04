namespace Services.DTOs.RequestDTOs
{
    public class PaymentRequestDTO
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionId { get; set; }
    }
}
