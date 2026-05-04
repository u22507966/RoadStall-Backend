using System.Text.Json.Serialization;

namespace RoadStallAPI.Models
{
    public class Role
    {

        public int Id { get; set; }
        public required string RoleName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<User>? Users { get; set; }
    }
}
