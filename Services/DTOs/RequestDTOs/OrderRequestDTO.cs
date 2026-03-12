namespace Services.DTOs.RequestDTOs
{
    public class OrderRequestDTO
    {
        public int UserId { get; set; }
        public string PaymentMethod { get; set; } = null!; // vnpay, zalopay, paypal
        public string BillingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
