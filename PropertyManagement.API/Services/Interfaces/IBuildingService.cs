using PropertyManagement.API.DTOs.Building;

namespace PropertyManagement.API.Services.Interfaces
{
    public interface IBuildingService
    {
        Task<IEnumerable<BuildingResponseDto>> GetAllAsync();
        Task<BuildingResponseDto?> GetByIdAsync(int id);
        Task<BuildingResponseDto> CreateAsync(CreateBuildingDto dto);
        Task<bool> UpdateAsync(int id, UpdateBuildingDto dto);
        Task<bool> DeleteAsync(int id);
    }
}