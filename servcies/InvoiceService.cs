using ERPtask.models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
namespace ERPtask.servcies
{
    public class InvoiceService
    {
        private readonly string _connectionString;
        private readonly TaxCalculationService _taxCalculationService;
        public InvoiceService(string connectionString, TaxCalculationService taxCalculationService)
        {
            _connectionString = connectionString;
            _taxCalculationService = taxCalculationService;
        }
        public Invoice CreateInvoice(int clientId, List<InvoiceItem> items, decimal discount)
        {
            if (clientId <= 0)
                throw new ArgumentException("Invalid client ID.");
            if (items == null || !items.Any())
                throw new ArgumentException("At least one item is required.");
            //if (discount < 0)
            //    throw new ArgumentException("Discount cannot be negative.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Retrieve client region
                var clientCommand = new SqlCommand(
                    "SELECT Region FROM Clients WHERE ClientID = @ClientID",
                    connection);
                clientCommand.Parameters.AddWithValue("@ClientID", clientId);
                string region;
                using (var reader = clientCommand.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new KeyNotFoundException("Client not found");
                    region = reader.IsDBNull(0) ? "Default" : reader.GetString(0);
                    Console.WriteLine(region);
                }

                decimal subtotal = items.Sum(item => item.Quantity * item.UnitPrice);
                var (taxAmount, total) = _taxCalculationService.CalculateTax(subtotal, region, discount);

                var command = new SqlCommand(
                    "INSERT INTO Invoices (ClientID, InvoiceDate, DueDate, Subtotal, Tax, Discount, Total) " +
                    "OUTPUT INSERTED.InvoiceID " +
                    "VALUES (@ClientID, @InvoiceDate, @DueDate, @Subtotal, @Tax, @Discount, @Total)",
                    connection);
                command.Parameters.AddWithValue("@ClientID", clientId);
                command.Parameters.AddWithValue("@InvoiceDate", DateTime.Now);
                command.Parameters.AddWithValue("@DueDate", DateTime.Now.AddDays(30));
                command.Parameters.AddWithValue("@Subtotal", subtotal);
                command.Parameters.AddWithValue("@Tax", taxAmount);
                command.Parameters.AddWithValue("@Discount", discount);
                command.Parameters.AddWithValue("@Total", total);
                int invoiceId = (int)command.ExecuteScalar();

                foreach (var item in items)
                {
                    var itemCommand = new SqlCommand(
                        "INSERT INTO InvoiceItems (InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice) " +
                        "VALUES (@InvoiceID, @ItemName, @Quantity, @UnitPrice, @TotalPrice)",
                        connection);
                    itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    itemCommand.Parameters.AddWithValue("@ItemName", item.ItemName);
                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCommand.Parameters.AddWithValue("@TotalPrice", item.Quantity * item.UnitPrice);
                    itemCommand.ExecuteNonQuery();
                }

                return new Invoice
                {
                    InvoiceID = invoiceId,
                    ClientID = clientId,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    Subtotal = subtotal,
                    Tax = taxAmount,
                    Discount = discount,
                    Total = total,
                    Items = items.Select(item => new InvoiceItem
                    {
                        InvoiceID = invoiceId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    }).ToList()
                };
            }
        }

        public Invoice EditInvoice(int invoiceId, List<InvoiceItem> updatedItems, decimal discount)
        {
            if (updatedItems == null || !updatedItems.Any())
                throw new ArgumentException("At least one item is required.");
            if (discount < 0)
                throw new ArgumentException("Discount cannot be negative.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT i.ClientID, i.InvoiceDate, i.DueDate, c.Region " +
                    "FROM Invoices i JOIN Clients c ON i.ClientID = c.ClientID " +
                    "WHERE i.InvoiceID = @InvoiceID",
                    connection);
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                int clientId;
                DateTime invoiceDate, dueDate;
                string region;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new KeyNotFoundException("Invoice not found");
                    clientId = reader.GetInt32(0);
                    invoiceDate = reader.GetDateTime(1);
                    dueDate = reader.GetDateTime(2);
                    region = reader.IsDBNull(3) ? "Default" : reader.GetString(3);
                }

                decimal subtotal = updatedItems.Sum(item => item.Quantity * item.UnitPrice);
                var (taxAmount, total) = _taxCalculationService.CalculateTax(subtotal, region, discount);

                var updateCommand = new SqlCommand(
                    "UPDATE Invoices SET Subtotal = @Subtotal, Tax = @Tax, Discount = @Discount, Total = @Total " +
                    "WHERE InvoiceID = @InvoiceID",
                    connection);
                updateCommand.Parameters.AddWithValue("@Subtotal", subtotal);
                updateCommand.Parameters.AddWithValue("@Tax", taxAmount);
                updateCommand.Parameters.AddWithValue("@Discount", discount);
                updateCommand.Parameters.AddWithValue("@Total", total);
                updateCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                updateCommand.ExecuteNonQuery();

                var deleteCommand = new SqlCommand(
                    "DELETE FROM InvoiceItems WHERE InvoiceID = @InvoiceID",
                    connection);
                deleteCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                deleteCommand.ExecuteNonQuery();

                foreach (var item in updatedItems)
                {
                    var itemCommand = new SqlCommand(
                        "INSERT INTO InvoiceItems (InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice) " +
                        "VALUES (@InvoiceID, @ItemName, @Quantity, @UnitPrice, @TotalPrice)",
                        connection);
                    itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    itemCommand.Parameters.AddWithValue("@ItemName", item.ItemName);
                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    itemCommand.Parameters.AddWithValue("@TotalPrice", item.Quantity * item.UnitPrice);
                    itemCommand.ExecuteNonQuery();
                }

                return new Invoice
                {
                    InvoiceID = invoiceId,
                    ClientID = clientId,
                    InvoiceDate = invoiceDate,
                    DueDate = dueDate,
                    Subtotal = subtotal,
                    Tax = taxAmount,
                    Discount = discount,
                    Total = total,
                    Items = updatedItems.Select(item => new InvoiceItem
                    {
                        InvoiceID = invoiceId,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    }).ToList()
                };
            }
        }


        public Invoice EditDiscountInvoice(int invoiceId, decimal discount)
        {
            if (discount < 0)
                throw new ArgumentException("Discount cannot be negative.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Retrieve existing invoice details
                var command = new SqlCommand(
                    "SELECT ClientID, InvoiceDate, DueDate, Subtotal, Tax, Discount, Total " +
                    "FROM Invoices WHERE InvoiceID = @InvoiceID",
                    connection);
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                int clientId;
                DateTime invoiceDate, dueDate;
                decimal subtotal, taxAmount, currentDiscount, total;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new KeyNotFoundException("Invoice not found");
                    clientId = reader.GetInt32(0);
                    invoiceDate = reader.GetDateTime(1);
                    dueDate = reader.GetDateTime(2);
                    subtotal = reader.GetDecimal(3);
                    taxAmount = reader.GetDecimal(4);
                    currentDiscount = reader.GetDecimal(5);
                    total = reader.GetDecimal(6);
                }

                // Calculate new total with updated discount
                total = subtotal + taxAmount - discount;

                // Update discount and total
                var updateCommand = new SqlCommand(
                    "UPDATE Invoices SET Discount = @Discount, Total = @Total " +
                    "WHERE InvoiceID = @InvoiceID",
                    connection);
                updateCommand.Parameters.AddWithValue("@Discount", discount);
                updateCommand.Parameters.AddWithValue("@Total", total);
                updateCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                updateCommand.ExecuteNonQuery();

                // Retrieve invoice items
                var items = new List<InvoiceItem>();
                var itemCommand = new SqlCommand(
                    "SELECT InvoiceItemID, InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice " +
                    "FROM InvoiceItems WHERE InvoiceID = @InvoiceID",
                    connection);
                itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                using (var reader = itemCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new InvoiceItem
                        {
                            InvoiceItemID = reader.GetInt32(0),
                            InvoiceID = reader.GetInt32(1),
                            ItemName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            UnitPrice = reader.GetDecimal(4),
                            TotalPrice = reader.GetDecimal(5)
                        });
                    }
                }

                return new Invoice
                {
                    InvoiceID = invoiceId,
                    ClientID = clientId,
                    InvoiceDate = invoiceDate,
                    DueDate = dueDate,
                    Subtotal = subtotal,
                    Tax = taxAmount,
                    Discount = discount,
                    Total = total,
                    Items = items
                };
            }
        }
        


        public List<Invoice> GetAllInvoices()
        {
            var invoices = new List<Invoice>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT InvoiceID, ClientID, InvoiceDate, DueDate, Subtotal, Tax, Discount, Total FROM Invoices",
                    connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoices.Add(new Invoice
                        {
                            InvoiceID = reader.GetInt32(0),
                            ClientID = reader.GetInt32(1),
                            InvoiceDate = reader.GetDateTime(2),
                            DueDate = reader.GetDateTime(3),
                            Subtotal = reader.GetDecimal(4),
                            Tax = reader.GetDecimal(5),
                            Discount = reader.GetDecimal(6),
                            Total = reader.GetDecimal(7),
                            Items = new List<InvoiceItem>()
                        });
                    }
                }

                foreach (var invoice in invoices)
                {
                    var itemCommand = new SqlCommand(
                        "SELECT InvoiceItemID, InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice FROM InvoiceItems WHERE InvoiceID = @InvoiceID",
                        connection);
                    itemCommand.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
                    using (var reader = itemCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoice.Items.Add(new InvoiceItem
                            {
                                InvoiceItemID = reader.GetInt32(0),
                                InvoiceID = reader.GetInt32(1),
                                ItemName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                UnitPrice = reader.GetDecimal(4),
                                TotalPrice = reader.GetDecimal(5)
                            });
                        }
                    }
                }
            }
            return invoices;
        }

        public Invoice GetInvoiceById(int invoiceId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT InvoiceID, ClientID, InvoiceDate, DueDate, Subtotal, Tax, Discount, Total FROM Invoices WHERE InvoiceID = @InvoiceID",
                    connection);
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                Invoice invoice = null;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        invoice = new Invoice
                        {
                            InvoiceID = reader.GetInt32(0),
                            ClientID = reader.GetInt32(1),
                            InvoiceDate = reader.GetDateTime(2),
                            DueDate = reader.GetDateTime(3),
                            Subtotal = reader.GetDecimal(4),
                            Tax = reader.GetDecimal(5),
                            Discount = reader.GetDecimal(6),
                            Total = reader.GetDecimal(7),
                            Items = new List<InvoiceItem>()
                        };
                    }
                }

                if (invoice == null) return null;

                var itemCommand = new SqlCommand(
                    "SELECT InvoiceItemID, InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice FROM InvoiceItems WHERE InvoiceID = @InvoiceID",
                    connection);
                itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                using (var reader = itemCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoice.Items.Add(new InvoiceItem
                        {
                            InvoiceItemID = reader.GetInt32(0),
                            InvoiceID = reader.GetInt32(1),
                            ItemName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            UnitPrice = reader.GetDecimal(4),
                            TotalPrice = reader.GetDecimal(5)
                        });
                    }
                }
                return invoice;
            }
        }

        public Invoice UpdateDueDate(int invoiceId, DateTime newDueDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Retrieve existing invoice details
                var command = new SqlCommand(
                    "SELECT ClientID, InvoiceDate, Subtotal, Tax, Discount, Total " +
                    "FROM Invoices WHERE InvoiceID = @InvoiceID", connection);
                command.Parameters.AddWithValue("@InvoiceID", invoiceId);

                int clientId;
                DateTime invoiceDate;
                decimal subtotal, tax, discount, total;

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new KeyNotFoundException("Invoice not found");

                    clientId = reader.GetInt32(0);
                    invoiceDate = reader.GetDateTime(1);
                    subtotal = reader.GetDecimal(2);
                    tax = reader.GetDecimal(3);
                    discount = reader.GetDecimal(4);
                    total = reader.GetDecimal(5);
                }

                // Update the due date
                var updateCommand = new SqlCommand(
                    "UPDATE Invoices SET DueDate = @DueDate WHERE InvoiceID = @InvoiceID", connection);
                updateCommand.Parameters.AddWithValue("@DueDate", newDueDate);
                updateCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                updateCommand.ExecuteNonQuery();

                // Retrieve invoice items
                var items = new List<InvoiceItem>();
                var itemCommand = new SqlCommand(
                    "SELECT InvoiceItemID, InvoiceID, ItemName, Quantity, UnitPrice, TotalPrice " +
                    "FROM InvoiceItems WHERE InvoiceID = @InvoiceID", connection);
                itemCommand.Parameters.AddWithValue("@InvoiceID", invoiceId);
                using (var reader = itemCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new InvoiceItem
                        {
                            InvoiceItemID = reader.GetInt32(0),
                            InvoiceID = reader.GetInt32(1),
                            ItemName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            UnitPrice = reader.GetDecimal(4),
                            TotalPrice = reader.GetDecimal(5)
                        });
                    }
                }

                return new Invoice
                {
                    InvoiceID = invoiceId,
                    ClientID = clientId,
                    InvoiceDate = invoiceDate,
                    DueDate = newDueDate,
                    Subtotal = subtotal,
                    Tax = tax,
                    Discount = discount,
                    Total = total,
                    Items = items
                };
            }
        }

    }
    public class TaxCalculationService
    {

        private readonly Dictionary<string, decimal> _regionalTaxRates;
        private readonly ILogger<TaxCalculationService> _logger;

        public TaxCalculationService(IConfiguration configuration, ILogger<TaxCalculationService> logger)
        {
            _logger = logger;
     
            //_regionalTaxRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            //{
            //    { "US-CA", 0.0875m }, // California, USA: 8.75%
            //    { "US-NY", 0.08875m }, // New York, USA: 8.875%
            //    { "UK", 0.20m }, // United Kingdom: 20% VAT
            //    { "CA-ON", 0.13m }, // Ontario, Canada: 13% HST
            //    { "DE", 0.19m }, // Germany: 19% VAT
            //    { "Default", 0.10m } // Default tax rate for unknown regions
            //};
            _regionalTaxRates = configuration.GetSection("RegionalTaxRates")
           .Get<Dictionary<string, decimal>>() ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            // Ensure default tax rate exists
            //if (!_regionalTaxRates.ContainsKey("Default"))
            //{
            //    _regionalTaxRates["Default"] = 0.10m;
            //    _logger.LogWarning("Default tax rate not found in configuration. Using fallback rate of 0.10.");
            //}

            //// Ensure case-insensitive keys
            //_regionalTaxRates = new Dictionary<string, decimal>(_regionalTaxRates, StringComparer.OrdinalIgnoreCase);
        }

        public (decimal TaxAmount, decimal UpdatedTotal) CalculateTax(decimal subtotal, string region, decimal discount)
        {
            if (subtotal < 0)
                throw new ArgumentException("Subtotal cannot be negative.");
            if (discount < 0)
                throw new ArgumentException("Discount cannot be negative.");

            // Normalize region input
            string normalizedRegion = string.IsNullOrWhiteSpace(region) ? "Default" : region.Trim();

            // Get tax rate, fallback to default if region not found
            if (!_regionalTaxRates.TryGetValue(normalizedRegion, out decimal taxRate))
            {
                taxRate = _regionalTaxRates[normalizedRegion];
                _logger.LogWarning("Region '{Region}' not found in tax rates. Using default tax rate of {DefaultRate}.", normalizedRegion, taxRate);
            }
           Console.WriteLine(taxRate);
            decimal taxAmount = subtotal * taxRate;
            decimal updatedTotal = subtotal + taxAmount - discount;
            
            return (taxAmount, updatedTotal);
        }
    }
}
