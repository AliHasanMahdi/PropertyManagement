using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.DTOs.Building;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        public BuildingsController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var buildings = await _buildingService.GetAllAsync();
            return Ok(buildings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var building = await _buildingService.GetByIdAsync(id);
            if (building == null)
                return NotFound(new { message = $"Building with ID {id} not found" });
            return Ok(building);
        }

        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Create([FromBody] CreateBuildingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var building = await _buildingService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBuildingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _buildingService.UpdateAsync(id, dto);
            if (!result)
                return NotFound(new { message = $"Building with ID {id} not found" });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _buildingService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = $"Building with ID {id} not found" });

            return NoContent();
        }
    }
}