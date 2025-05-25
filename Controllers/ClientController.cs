using ERPtask.servcies;
using Microsoft.AspNetCore.Mvc;

namespace ERPtask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ClientService _clientService;

        public ClientController(ClientService clientService)
        {
            _clientService = clientService;
        }
        [HttpPut("editClient/{id}")]
        public IActionResult UpdateClient(int id, [FromBody] ClientRequest request)// This method needs to be updated to support client regoin
        {
            var existingClient = _clientService.GetClientById(id);
            if (existingClient == null)
                return NotFound("Client not found.");

            var success = _clientService.UpdateClient(id, request.ClientName, request.ContactDetails);
            if (success)
            {
                var updatedClient = _clientService.GetClientById(id);
                return Ok(updatedClient);
            }

            return StatusCode(500, "An error occurred while updating the client.");
        }


        [HttpPost]
        public IActionResult AddClient([FromBody] ClientRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientName) || string.IsNullOrWhiteSpace(request.Region))
                return BadRequest("Client name and region are required.");
            var client = _clientService.AddClient(request.ClientName, request.ContactDetails, request.Region);
            return CreatedAtAction(nameof(GetClientById), new { id = client.ClientID }, client);
        }

        [HttpGet]
        public IActionResult GetAllClients()
        {
            var clients = _clientService.GetAllClients();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public IActionResult GetClientById(int id)
        {
            var client = _clientService.GetClientById(id);
            if (client == null) return NotFound();
            return Ok(client);
        }
    }

    public class ClientRequest
    {
        public string ClientName { get; set; }
        public string ContactDetails { get; set; }
        public string Region { get; set; }
    }
}
