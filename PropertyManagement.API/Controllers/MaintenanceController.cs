using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.DTOs.Maintenance;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _maintenanceService.GetAllAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _maintenanceService.GetByIdAsync(id);
            if (request == null)
                return NotFound(new { message = $"Request with ID {id} not found" });
            return Ok(request);
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> Lookup([FromQuery] string ticketNumber, [FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber) || string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { message = "Ticket number and phone are required" });

            var request = await _maintenanceService.LookupAsync(ticketNumber, phone);
            if (request == null)
                return NotFound(new { message = "No request found with the provided details" });

            return Ok(request);
        }

        [HttpPost]
        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = await _maintenanceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "PropertyManager,MaintenanceStaff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMaintenanceStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceService.UpdateStatusAsync(id, dto.Status);
            if (!result)
                return BadRequest(new { message = "Invalid status or request not found" });

            return NoContent();
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignStaffDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceService.AssignStaffAsync(id, dto.StaffId);
            if (!result)
                return NotFound(new { message = "Request or staff not found" });

            return NoContent();
        }
    }
}