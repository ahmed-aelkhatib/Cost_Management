using Microsoft.AspNetCore.Mvc;

namespace ERPtask.Controllers
{
    using ERPtask.servcies;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class ReminderController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public ReminderController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public IActionResult SendDueReminder([FromBody] ReminderRequest request)
        {
            try
            {
                var notification = _notificationService.SendDueReminder(request.InvoiceID, request.NotificationType);
                return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotificationID }, notification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAllNotifications()
        {
            var notifications = _notificationService.GetAllNotifications();
            return Ok(notifications);
        }

        [HttpGet("{id}")]
        public IActionResult GetNotificationById(int id)
        {
            var notification = _notificationService.GetNotificationById(id);
            if (notification == null) return NotFound();
            return Ok(notification);
        }
    }

    public class ReminderRequest
    {
        public int InvoiceID { get; set; }
        public string NotificationType { get; set; } = "email";
    }
}
