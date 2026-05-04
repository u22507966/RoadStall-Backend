namespace RoadStallAPI.Models.DTOs
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
        public required string Token { get; set; }
    }
}
