using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Status { get; set; } = 0;

        //foreing key to role
        public int RoleId { get; set; } = 0;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Role? Role { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<StockTake>? StockTakes { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<StockChange>? StockChanges { get; set; }
    }
}
