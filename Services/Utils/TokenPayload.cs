namespace Services.Utils
{
    public class TokenPayload
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
