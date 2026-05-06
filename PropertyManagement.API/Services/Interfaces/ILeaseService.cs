using PropertyManagement.API.DTOs.Lease;

namespace PropertyManagement.API.Services.Interfaces
{
    public interface ILeaseService
    {
        Task<IEnumerable<LeaseResponseDto>> GetAllAsync();
        Task<LeaseResponseDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, LeaseResponseDto? Lease)> CreateAsync(CreateLeaseDto dto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> AddPaymentAsync(int leaseId, AddPaymentDto dto);
    }
}