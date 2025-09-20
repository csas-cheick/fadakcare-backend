using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetByUser(int userId, [FromQuery] int take = 20, [FromQuery] int skip = 0)
        {
            Console.WriteLine($"Getting notifications for user {userId}");
            var items = await _service.GetByUserAsync(userId, take, skip);
            Console.WriteLine($"Found {items.Count()} notifications");
            return Ok(items);
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<object>> CountUnread(int userId)
        {
            Console.WriteLine($"Getting unread count for user {userId}");
            var count = await _service.CountUnreadAsync(userId);
            Console.WriteLine($"Unread count: {count}");
            return Ok(new { count });
        }

        [HttpPost]
        public async Task<ActionResult<Notification>> Create([FromBody] Notification notification)
        {
            var created = await _service.CreateAsync(notification);
            return Ok(created);
        }

        [HttpPost("user/{userId}/mark-read/{id}")]
        public async Task<IActionResult> MarkOne(int userId, int id)
        {
            await _service.MarquerCommeLuAsync(id, userId);
            return NoContent();
        }

        [HttpPost("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAll(int userId)
        {
            await _service.MarquerToutCommeLuAsync(userId);
            return NoContent();
        }
    }
}
