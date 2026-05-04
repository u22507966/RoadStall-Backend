namespace RoadStallAPI.Models.DTOs
{
    public class UpdateUserRequest
    {
        public required string Username { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; }
    }
}
