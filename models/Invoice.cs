namespace ERPtask.models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int ClientID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public List<InvoiceItem> Items { get; set; }
    }
}
