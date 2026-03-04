namespace Services.DTOs.ResponseDTOs
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = null!; // pending, confirmed, shipped, delivered, cancelled
        public string PaymentMethod { get; set; } = null!;
        public string BillingAddress { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
        public PaymentResponseDTO? Payment { get; set; }
    }

    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
    }
}
