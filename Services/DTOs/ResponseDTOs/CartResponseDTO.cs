namespace Services.DTOs.ResponseDTOs
{
    public class CartResponseDTO
    {
        public int CartId { get; set; }

        public int? UserId { get; set; }

        public List<CartItemResponseDTO> CartItems { get; set; } = new();

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = null!;
    }
}
