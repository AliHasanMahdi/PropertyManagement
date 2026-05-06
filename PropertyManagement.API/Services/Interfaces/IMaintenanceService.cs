using PropertyManagement.API.DTOs.Maintenance;

namespace PropertyManagement.API.Services.Interfaces
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceResponseDto>> GetAllAsync();
        Task<MaintenanceResponseDto?> GetByIdAsync(int id);
        Task<MaintenanceResponseDto?> LookupAsync(string ticketNumber, string phone);
        Task<MaintenanceResponseDto> CreateAsync(CreateMaintenanceRequestDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> AssignStaffAsync(int id, int staffId);
    }
}