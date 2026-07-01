using LabInventory.Models.DTOs.Labs;

namespace LabInventory.Services.Interfaces;

public interface ILabService
{
    Task<List<LabDto>> GetAllAsync(int requestingUserId, bool isAdmin);
    Task<LabDto?> GetByIdAsync(int labId);
    Task<LabDto> CreateAsync(CreateLabDto dto, int createdByUserId);
    Task<LabDto> UpdateAsync(int labId, CreateLabDto dto);
    Task DeleteAsync(int labId);
    Task<List<object>> GetLabUsersAsync(int labId);
}