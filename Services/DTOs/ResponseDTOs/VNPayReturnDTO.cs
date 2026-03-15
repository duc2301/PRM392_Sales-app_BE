namespace Services.DTOs.ResponseDTOs
{
    public class VNPayReturnDTO
    {
        public int OrderId { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string? ResponseCode { get; set; }
        public string? TransactionId { get; set; }
    }
}
