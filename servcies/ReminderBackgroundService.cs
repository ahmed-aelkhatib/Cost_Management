using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace ERPtask.servcies
{


    public class ReminderBackgroundService : BackgroundService
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly TimeSpan _delay;

        public ReminderBackgroundService(NotificationService notificationService, ILogger<ReminderBackgroundService> logger, IConfiguration configuration)
        {
            _notificationService = notificationService;
            _logger = logger;
            _delay = TimeSpan.FromHours(configuration.GetValue<double>("ReminderIntervalHours", 24)); // default 24
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking for invoice reminders at {time}", DateTime.Now);
                    var notifications = _notificationService.CheckAndSendReminders();
                    if (notifications.Any())
                    {
                        _logger.LogInformation("Sent {count} reminders", notifications.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminders");
                }

              
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

    }

}
