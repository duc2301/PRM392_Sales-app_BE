namespace Services.DTOs.ResponseDTOs
{
    public class AuthResponseDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
