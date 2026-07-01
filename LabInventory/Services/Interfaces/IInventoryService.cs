using LabInventory.Models.DTOs.Inventory;

namespace LabInventory.Services.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItemDto>> GetAllAsync(int? labId, string? search, List<int>? allowedLabIds = null);
    Task<InventoryItemDto?> GetByIdAsync(int itemId);
    Task<object> GetIssuanceHistoryAsync(int itemId);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto, int createdByUserId);
    Task<InventoryItemDto> UpdateAsync(int itemId, CreateInventoryItemDto dto);
    Task DeleteAsync(int itemId);
}