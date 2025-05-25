using ERPtask.models;
using System.Data.SqlClient;
using System.Net.Mail;
namespace ERPtask.servcies
{
    public class NotificationService
    {
        private readonly string _connectionString;

        public NotificationService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Notification SendDueReminder(int invoiceId, string notificationType)
        {
            if (!new[] { "email", "sms", "in-app" }.Contains(notificationType.ToLower()))
                throw new ArgumentException("Invalid notification type. Use 'email', 'sms', or 'in-app'.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT i.DueDate, c.ClientName, c.ContactDetails, i.ClientID " +
                    "FROM Invoices i JOIN Clients c ON i.ClientID = c.ClientID " +
                    "WHERE i.InvoiceID = @InvoiceID",
                    connection);
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);

                int clientId;
                DateTime dueDate;
                string clientName, contactDetails;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) throw new KeyNotFoundException("Invoice not found");
                    dueDate = reader.GetDateTime(0);
                    clientName = reader.GetString(1);
                    contactDetails = reader.GetString(2); 
                    clientId = reader.GetInt32(3);
                }

                string message = dueDate < DateTime.Now
                    ? $"Dear {clientName}, your invoice {invoiceId} was due on {dueDate.ToShortDateString()} and is now overdue."
                    : $"Dear {clientName}, your invoice {invoiceId} is due on {dueDate.ToShortDateString()}.";

                
                if (notificationType.ToLower() == "email")
                {
                    SendEmail(contactDetails, "Invoice Due Reminder", message);
                }

                var notification = new Notification
                {
                    InvoiceID = invoiceId,
                    ClientID = clientId,
                    Type = notificationType.ToLower(),
                    Message = message,
                    SentDateTime = DateTime.Now
                };

                var insertCommand = new SqlCommand(
                    "INSERT INTO Notifications (InvoiceID, ClientID, Type, Message, SentDateTime) " +
                    "OUTPUT INSERTED.NotificationID " +
                    "VALUES (@InvoiceID, @ClientID, @Type, @Message, @SentDateTime)",
                    connection);
                insertCommand.Parameters.AddWithValue("@InvoiceID", notification.InvoiceID);
                insertCommand.Parameters.AddWithValue("@ClientID", notification.ClientID);
                insertCommand.Parameters.AddWithValue("@Type", notification.Type);
                insertCommand.Parameters.AddWithValue("@Message", notification.Message);
                insertCommand.Parameters.AddWithValue("@SentDateTime", notification.SentDateTime);
                notification.NotificationID = (int)insertCommand.ExecuteScalar();

                return notification;
            }
        }

        public List<Notification> GetAllNotifications()
        {
            var notifications = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT NotificationID, InvoiceID, ClientID, Type, Message, SentDateTime " +
                    "FROM Notifications",
                    connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new Notification
                        {
                            NotificationID = reader.GetInt32(0),
                            InvoiceID = reader.GetInt32(1),
                            ClientID = reader.GetInt32(2),
                            Type = reader.GetString(3),
                            Message = reader.GetString(4),
                            SentDateTime = reader.GetDateTime(5)
                        });
                    }
                }
            }
            return notifications;
        }

        public Notification GetNotificationById(int notificationId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT NotificationID, InvoiceID, ClientID, Type, Message, SentDateTime " +
                    "FROM Notifications WHERE NotificationID = @NotificationID",
                    connection);
                command.Parameters.AddWithValue("@NotificationID", notificationId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Notification
                        {
                            NotificationID = reader.GetInt32(0),
                            InvoiceID = reader.GetInt32(1),
                            ClientID = reader.GetInt32(2),
                            Type = reader.GetString(3),
    

                            Message = reader.GetString(4),
                            SentDateTime = reader.GetDateTime(5)
                        };
                    }
                    return null;
                }
            }
        }

        public List<Notification> CheckAndSendReminders()
        {
            var notifications = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT i.InvoiceID, i.DueDate, c.ClientName, c.ContactDetails, i.ClientID " +
                    "FROM Invoices i JOIN Clients c ON i.ClientID = c.ClientID " +
                    "WHERE i.DueDate <= @UpcomingDate AND i.DueDate >= @PastDate",
                    connection);
                command.Parameters.AddWithValue("@UpcomingDate", DateTime.Now.AddDays(7));
                command.Parameters.AddWithValue("@PastDate", DateTime.Now.AddDays(-7));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int invoiceId = reader.GetInt32(0);
                        DateTime dueDate = reader.GetDateTime(1);
                        string clientName = reader.GetString(2);
                        string contactDetails = reader.GetString(3);
                        int clientId = reader.GetInt32(4);

                        string message = dueDate < DateTime.Now
                            ? $"Dear {clientName}, your invoice {invoiceId} was due on {dueDate.ToShortDateString()} and is now overdue."
                            : $"Dear {clientName}, your invoice {invoiceId} is due on {dueDate.ToShortDateString()}.";

                        
                        SendEmail(contactDetails, "Invoice Due Reminder", message);

                        
                        var notification = new Notification
                        {
                            InvoiceID = invoiceId,
                            ClientID = clientId,
                            Type = "email",
                            Message = message,
                            SentDateTime = DateTime.Now
                        };

                        var insertCommand = new SqlCommand(
                            "INSERT INTO Notifications (InvoiceID, ClientID, Type, Message, SentDateTime) " +
                            "OUTPUT INSERTED.NotificationID " +
                            "VALUES (@InvoiceID, @ClientID, @Type, @Message, @SentDateTime)",
                            connection);
                        insertCommand.Parameters.AddWithValue("@InvoiceID", notification.InvoiceID);
                        insertCommand.Parameters.AddWithValue("@ClientID", notification.ClientID);
                        insertCommand.Parameters.AddWithValue("@Type", notification.Type);
                        insertCommand.Parameters.AddWithValue("@Message", message);
                        insertCommand.Parameters.AddWithValue("@SentDateTime", notification.SentDateTime);
                        notification.NotificationID = (int)insertCommand.ExecuteScalar();

                        notifications.Add(notification);
                    }
                }
            }
            return notifications;
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = "fcishotelmanagementsystem@gmail.com";
            var password = "uzeu tfki tqzn xipq";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, toEmail, subject, body);
            smtp.Send(mail);
        }
    }
}
