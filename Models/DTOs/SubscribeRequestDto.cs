namespace RoadStallAPI.Models.DTOs
{
    public class SubscribeRequestDto
    {
        public PushSubDto Subscription { get; set; } = new();
        public int UserId { get; set; }
    }
}
