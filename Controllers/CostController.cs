using Microsoft.AspNetCore.Mvc;
using ERPtask.servcies;
namespace ERPtask.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class CostController : ControllerBase
    {
        private readonly CostService _costService;

        public CostController(CostService costService)
        {
            _costService = costService;
        }

        [HttpPost]
        public IActionResult AddCostEntry([FromBody] CostEntryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Category) || request.Amount < 0)
                return BadRequest("Invalid category or amount.");
            var costEntry = _costService.AddCostEntry(request.Category, request.Amount, request.Date, request.Description);
            return CreatedAtAction(nameof(GetCostById), new { id = costEntry.CostID }, costEntry);
        }

        [HttpGet]
        public IActionResult GetAllCosts()
        {
            var costs = _costService.GetAllCosts();
            return Ok(costs);
        }

        [HttpGet("{id}")]
        public IActionResult GetCostById(int id)
        {
            var cost = _costService.GetCostById(id);
            if (cost == null) return NotFound();
            return Ok(cost);
        }
    }

    public class CostEntryRequest
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
