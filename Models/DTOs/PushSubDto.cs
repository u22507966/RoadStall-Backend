using RoadStallAPI.Models.DTOs;

namespace RoadStallAPI.Models.DTOs
{
    public class PushSubDto
    {

        public string Endpoint { get; set; } = string.Empty;
        public PushKeysDto Keys { get; set; } = new();

    }
}
