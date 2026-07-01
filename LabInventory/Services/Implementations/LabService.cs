using LabInventory.Data;
using LabInventory.Models.DTOs.Labs;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class LabService : ILabService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public LabService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<LabDto>> GetAllAsync(int requestingUserId, bool isAdmin)
    {
        var query = _db.Labs.Include(l => l.CreatedByUser).AsQueryable();

        if (!isAdmin)
        {
            var assignedLabIds = await _db.UserLabs
                .Where(ul => ul.UserId == requestingUserId)
                .Select(ul => ul.LabId)
                .ToListAsync();

            query = query.Where(l => assignedLabIds.Contains(l.LabId));
        }

        return await query.Select(l => new LabDto
        {
            LabId = l.LabId,
            LabName = l.LabName,
            Location = l.Location,
            CreatedByName = l.CreatedByUser.FullName,
            CreatedAt = l.CreatedAt
        }).ToListAsync();
    }

    public async Task<LabDto?> GetByIdAsync(int labId)
    {
        return await _db.Labs
            .Include(l => l.CreatedByUser)
            .Where(l => l.LabId == labId)
            .Select(l => new LabDto
            {
                LabId = l.LabId,
                LabName = l.LabName,
                Location = l.Location,
                CreatedByName = l.CreatedByUser.FullName,
                CreatedAt = l.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LabDto> CreateAsync(CreateLabDto dto, int createdByUserId)
    {
        var lab = new Lab
        {
            LabName = dto.LabName,
            Location = dto.Location,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Labs.Add(lab);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
            createdByUserId, "Created", "Labs",
            lab.LabId.ToString(),
            newValues: $"{{LabName: \"{lab.LabName}\"}}");

        return await GetByIdAsync(lab.LabId)
            ?? throw new InvalidOperationException("Lab not found after creation.");
    }

    public async Task<LabDto> UpdateAsync(int labId, CreateLabDto dto)
    {
        var lab = await _db.Labs.FindAsync(labId)
            ?? throw new InvalidOperationException("Lab not found.");

        lab.LabName = dto.LabName;
        lab.Location = dto.Location;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(labId)
            ?? throw new InvalidOperationException("Lab not found after update.");
    }

    public async Task DeleteAsync(int labId)
    {
        var lab = await _db.Labs.FindAsync(labId)
            ?? throw new InvalidOperationException("Lab not found.");

        bool hasItems = await _db.InventoryItems.AnyAsync(i => i.LabId == labId);
        if (hasItems)
            throw new InvalidOperationException(
                "Cannot delete a lab that has inventory items. Remove all items first.");

        _db.Labs.Remove(lab);
        await _db.SaveChangesAsync();
    }

    public async Task<List<object>> GetLabUsersAsync(int labId)
    {
        return await _db.UserLabs
            .Where(ul => ul.LabId == labId)
            .Select(ul => (object)new
            {
                ul.User.UserId,
                ul.User.FullName,
                ul.User.Email,
                ul.AssignedAt
            })
            .ToListAsync();
    }
}