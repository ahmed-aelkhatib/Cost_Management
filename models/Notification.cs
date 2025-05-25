namespace ERPtask.models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int InvoiceID { get; set; }
        public int ClientID { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime SentDateTime { get; set; }
    }
}
