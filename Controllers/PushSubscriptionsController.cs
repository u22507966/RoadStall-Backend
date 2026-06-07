using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadStallAPI;
using RoadStallAPI.Models;
using RoadStallAPI.Models.DTOs;
using WebPush;                      //this is the library for sending web push notifications, you need to install it from NuGet (WebPush)

namespace RoadStallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushSubscriptionsController : Controller
    {

        private readonly RoadStallDbContext _context;
        private readonly string _publicKey = "BHsRGFM108YdZjmoUupXm7A48gxA-7QtbsHT2m6R0--xDPe6zl333fFBPa_IiGI0KLAhbtKDyrSTmE2RXrc4kw8";
        private readonly string _privateKey = "VrPlc_HvIRJGcU_KeRqTUZvs4kewt6mF6CZEapG6_oU";

        public PushSubscriptionsController(RoadStallDbContext context)
        {
            _context = context;
        }

        //[HttpGet("checkSubscription")]
        //public async Task<ActionResult> checkSubscription([FromBody] PushSubDto sub)
        //{

        //}

        [HttpPost("subscribe")]
        public async Task<ActionResult> Subscribe([FromBody] PushSubDto sub, [FromBody] int userId)
        {
            var exists = _context.PushSubscriptions.Any(x => x.Endpoint == sub.Endpoint);

            if (!exists)
            {
                _context.PushSubscriptions.Add(new PushSubscriptions
                {
                    UserId = userId,
                    Endpoint = sub.Endpoint,
                    P256dh = sub.Keys.P256dh,
                    Auth = sub.Keys.Auth
                });

                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Subscription successful from backend" });
        }

        [HttpPost("sendToAll")]
        public async Task<ActionResult> SendToAll([FromBody] NotificationDto notification)
        {
            var subscriptions = await _context.PushSubscriptions.ToListAsync();

            var vapidDetails = new VapidDetails(
                "mailto:youremail@example.com",
                _publicKey,
                _privateKey
            );

            var webPushClient = new WebPushClient();

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                notification = new
                {
                    title = notification.Title,
                    body = notification.Message,
                    //icon = "assets/icons/icon-192x192.png"
                }
            });

            foreach (var sub in subscriptions)
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);

                try
                {
                    await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
                }
                catch (WebPushException ex)
                {
                    Console.WriteLine($"Error sending notification: {ex.Message}");
                }
            }

            return Ok(new { message = "Notifications sent to all subscribers" });
        }

        [HttpDelete("deleteSubscription/{id}")]
        public async Task<ActionResult> deleteSubscription(int id)
        {
            var sub = await _context.PushSubscriptions.FindAsync(id);
            if (sub != null)
            {
                _context.PushSubscriptions.Remove(sub);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Subscription deleted" });
            }
            return NotFound(new { message = "No notification subscriptions found" });
        }

        [HttpDelete("clearSubscriptions")]
        public async Task<ActionResult> clearSubscriptions()
        {
            var allSubscriptions = await _context.PushSubscriptions.ToListAsync();
            _context.PushSubscriptions.RemoveRange(allSubscriptions);
            await _context.SaveChangesAsync();
            return Ok(new { message = "All subscriptions cleared" });
        }




    }
}
