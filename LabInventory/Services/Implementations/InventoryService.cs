using LabInventory.Data;
using LabInventory.Models.DTOs.Inventory;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public InventoryService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<InventoryItemDto>> GetAllAsync(int? labId, string? search, List<int>? allowedLabIds = null)
    {
        var query = _db.InventoryItems.Include(i => i.Lab).AsQueryable();

        if (labId.HasValue)
            query = query.Where(i => i.LabId == labId.Value);
        else if (allowedLabIds != null && allowedLabIds.Any())
            query = query.Where(i => allowedLabIds.Contains(i.LabId));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i =>
                i.EquipmentName.Contains(search) ||
                (i.ModelNumber != null && i.ModelNumber.Contains(search)));

        return await query.Select(i => new InventoryItemDto
        {
            ItemId = i.ItemId,
            LabId = i.LabId,
            LabName = i.Lab.LabName,
            EquipmentName = i.EquipmentName,
            ModelNumber = i.ModelNumber,
            Placement = i.Placement,
            FinePerDay = i.FinePerDay,
            TotalQuantity = i.TotalQuantity,
            IssuedQuantity = i.IssuedQuantity,
            DefectiveQuantity = i.DefectiveQuantity,
            LostQuantity = i.LostQuantity,
            RemainingQuantity = i.RemainingQuantity,
            CreatedAt = i.CreatedAt
        }).ToListAsync();
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int itemId)
    {
        return await _db.InventoryItems
            .Include(i => i.Lab)
            .Where(i => i.ItemId == itemId)
            .Select(i => new InventoryItemDto
            {
                ItemId = i.ItemId,
                LabId = i.LabId,
                LabName = i.Lab.LabName,
                EquipmentName = i.EquipmentName,
                ModelNumber = i.ModelNumber,
                Placement = i.Placement,
                FinePerDay = i.FinePerDay,
                TotalQuantity = i.TotalQuantity,
                IssuedQuantity = i.IssuedQuantity,
                DefectiveQuantity = i.DefectiveQuantity,
                LostQuantity = i.LostQuantity,
                RemainingQuantity = i.RemainingQuantity,
                CreatedAt = i.CreatedAt
            }).FirstOrDefaultAsync();
    }

    public async Task<object> GetIssuanceHistoryAsync(int itemId)
    {
        var students = await _db.StudentIssuances
            .Where(x => x.ItemId == itemId)
            .Select(x => new
            {
                Type = "Student",
                Name = x.Student1Name,
                x.QuantityIssued,
                x.IssueDate,
                x.DueDate,
                x.ReturnDate,
                x.ConditionOnReturn,
                x.FineAmount,
                x.Status
            }).ToListAsync();

        var employees = await _db.EmployeeIssuances
            .Where(x => x.ItemId == itemId)
            .Select(x => new
            {
                Type = "Employee",
                Name = x.FacultyName,
                x.QuantityIssued,
                x.IssueDate,
                x.DueDate,
                x.ReturnDate,
                x.ConditionOnReturn,
                x.FineAmount,
                x.Status
            }).ToListAsync();

        return new { students, employees };
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto, int createdByUserId)
    {
        var labExists = await _db.Labs.AnyAsync(l => l.LabId == dto.LabId);
        if (!labExists)
            throw new InvalidOperationException("Lab not found.");

        var item = new InventoryItem
        {
            LabId = dto.LabId,
            EquipmentName = dto.EquipmentName,
            ModelNumber = dto.ModelNumber,
            Placement = dto.Placement,
            FinePerDay = dto.FinePerDay,
            TotalQuantity = dto.TotalQuantity,
            IssuedQuantity = 0,
            DefectiveQuantity = 0,
            LostQuantity = 0,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(createdByUserId, "Created", "InventoryItems",
            item.ItemId.ToString(),
            newValues: $"{{Equipment: {item.EquipmentName}, Total: {item.TotalQuantity}}}");

        return await GetByIdAsync(item.ItemId)
            ?? throw new InvalidOperationException("Item not found after creation.");
    }

    public async Task<InventoryItemDto> UpdateAsync(int itemId, CreateInventoryItemDto dto)
    {
        var item = await _db.InventoryItems.FindAsync(itemId)
            ?? throw new InvalidOperationException("Item not found.");

        int committed = item.IssuedQuantity + item.DefectiveQuantity + item.LostQuantity;
        if (dto.TotalQuantity < committed)
            throw new InvalidOperationException(
                $"Cannot reduce total quantity below {committed} (already issued/defective/lost).");

        item.EquipmentName = dto.EquipmentName;
        item.ModelNumber = dto.ModelNumber;
        item.Placement = dto.Placement;
        item.FinePerDay = dto.FinePerDay;
        item.TotalQuantity = dto.TotalQuantity;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(itemId)
            ?? throw new InvalidOperationException("Item not found after update.");
    }

    public async Task DeleteAsync(int itemId)
    {
        var item = await _db.InventoryItems.FindAsync(itemId)
            ?? throw new KeyNotFoundException("Item not found.");

        if (item.IssuedQuantity > 0)
            throw new InvalidOperationException("Cannot delete item with active issuances.");

        try
        {
            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                "Cannot delete — this item is referenced by other records (e.g. issuance history).");
        }
    }
}