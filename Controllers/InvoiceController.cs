using Microsoft.AspNetCore.Mvc;
using ERPtask.models;
using ERPtask.servcies;
using Microsoft.AspNetCore.Mvc;
namespace ERPtask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoiceController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        public IActionResult CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            if (request.ClientID <= 0 || request.Items == null || !request.Items.Any() || request.Discount < 0)
                return BadRequest("Invalid client ID, items, or discount.");
            try
            {
                var invoice = _invoiceService.CreateInvoice(request.ClientID, request.Items, request.Discount);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.InvoiceID }, invoice);
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

        [HttpPut("{id}")]
        public IActionResult EditInvoice(int id, [FromBody] EditInvoiceRequest request)
        {
            if (request.Items == null || !request.Items.Any() || request.Discount < 0)
                return BadRequest("Invalid items or discount.");
            try
            {
                var invoice = _invoiceService.EditInvoice(id, request.Items, request.Discount);
                return Ok(invoice);
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

        [HttpPut("editInvoice/{id}")]
        public IActionResult EditDiscountInvoice(int id, [FromBody] EditDiscountInvoiceRequest request)
        {
            if (request.Discount < 0)
                return BadRequest("Invalid items, tax rate, or discount.");
            try
            {
                var invoice = _invoiceService.EditDiscountInvoice(id, request.Discount);
                return Ok(invoice);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("allInvoices")]
        public IActionResult GetAllInvoices()
        {
            var invoices = _invoiceService.GetAllInvoices();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public IActionResult GetInvoiceById(int id)
        {
            var invoice = _invoiceService.GetInvoiceById(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpPut("updateDueDate/{id}")]
        public IActionResult UpdateDueDate(int id, [FromBody] UpdateDueDateRequest request)
        {
            if (request.NewDueDate < DateTime.Today)
                return BadRequest("Due date cannot be in the past.");

            try
            {
                var invoice = _invoiceService.UpdateDueDate(id, request.NewDueDate);
                return Ok(invoice);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }


    public class CreateInvoiceRequest
    {
        public int ClientID { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public decimal Discount { get; set; }
    }

    public class EditInvoiceRequest
    {
        public List<InvoiceItem> Items { get; set; }
        public decimal Discount { get; set; }
    }

    public class EditDiscountInvoiceRequest
    {
        public decimal Discount { get; set; }
    }
    public class UpdateDueDateRequest
    {
        public DateTime NewDueDate { get; set; }
    }

}
