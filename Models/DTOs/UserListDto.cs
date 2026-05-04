namespace RoadStallAPI.Models.DTOs
{
    public class UserListDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public int Status { get; set; }
        public int RoleId { get; set; }
    }
}
