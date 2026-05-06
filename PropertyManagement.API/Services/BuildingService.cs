using Microsoft.EntityFrameworkCore;
using PropertyManagement.API.Data;
using PropertyManagement.API.DTOs.Building;
using PropertyManagement.API.Models;
using PropertyManagement.API.Services.Interfaces;

namespace PropertyManagement.API.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly AppDbContext _context;

        public BuildingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BuildingResponseDto>> GetAllAsync()
        {
            return await _context.Buildings
                .Include(b => b.Units)
                .Select(b => new BuildingResponseDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    City = b.City,
                    Type = b.Type,
                    TotalUnits = b.Units.Count,
                    AvailableUnits = b.Units.Count(u => u.Status == "Available")
                })
                .ToListAsync();
        }

        public async Task<BuildingResponseDto?> GetByIdAsync(int id)
        {
            var building = await _context.Buildings
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null) return null;

            return new BuildingResponseDto
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                City = building.City,
                Type = building.Type,
                TotalUnits = building.Units.Count,
                AvailableUnits = building.Units.Count(u => u.Status == "Available")
            };
        }

        public async Task<BuildingResponseDto> CreateAsync(CreateBuildingDto dto)
        {
            var building = new Building
            {
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City,
                Type = dto.Type
            };

            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();

            return new BuildingResponseDto
            {
                Id = building.Id,
                Name = building.Name,
                Address = building.Address,
                City = building.City,
                Type = building.Type,
                TotalUnits = 0,
                AvailableUnits = 0
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateBuildingDto dto)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return false;

            building.Name = dto.Name;
            building.Address = dto.Address;
            building.City = dto.City;
            building.Type = dto.Type;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return false;

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}