namespace RoadStallAPI.Models.DTOs
{
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
