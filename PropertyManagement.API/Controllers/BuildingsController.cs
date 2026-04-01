using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.Models;

namespace PropertyManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BuildingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuildingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var buildings = await _context.Buildings.Include(b => b.Units).ToListAsync();
            return Ok(buildings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var building = await _context.Buildings.Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (building == null) return NotFound();
            return Ok(building);
        }

        [HttpPost]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Create([FromBody] Building building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Update(int id, [FromBody] Building building)
        {
            if (id != building.Id) return BadRequest();
            _context.Entry(building).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "PropertyManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();
            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}