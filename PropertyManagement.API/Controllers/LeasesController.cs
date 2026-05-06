using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.DTOs.Lease;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeasesController : ControllerBase
    {
        private readonly ILeaseService _leaseService;

        public LeasesController(ILeaseService leaseService)
        {
            _leaseService = leaseService;
        }

        [HttpGet]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> GetAll()
        {
            var leases = await _leaseService.GetAllAsync();
            return Ok(leases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lease = await _leaseService.GetByIdAsync(id);
            if (lease == null)
                return NotFound(new { message = $"Lease with ID {id} not found" });
            return Ok(lease);
        }

        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Create([FromBody] CreateLeaseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, lease) = await _leaseService.CreateAsync(dto);
            if (!success)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetById), new { id = lease!.Id }, lease);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLeaseStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _leaseService.UpdateStatusAsync(id, dto.Status);
            if (!result)
                return BadRequest(new { message = "Invalid status or lease not found" });

            return NoContent();
        }

        [HttpPost("{id}/payments")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> AddPayment(int id, [FromBody] AddPaymentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _leaseService.AddPaymentAsync(id, dto);
            if (!result)
                return NotFound(new { message = $"Lease with ID {id} not found" });

            return Ok(new { message = "Payment recorded successfully" });
        }
    }
}