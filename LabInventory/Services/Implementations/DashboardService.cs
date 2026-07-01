using LabInventory.Data;
using LabInventory.Models.DTOs.Dashboard;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int userId, bool isAdmin, List<int>? allowedLabIds = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // ── Inventory items scoped to allowed labs ─────────────────
        var itemQuery = _db.InventoryItems.AsQueryable();
        if (allowedLabIds != null)
            itemQuery = itemQuery.Where(i => allowedLabIds.Contains(i.LabId));

        var items = await itemQuery.ToListAsync();

        // ── Overdue counts scoped to allowed labs ──────────────────
        var studentOverdueQuery = _db.StudentIssuances
            .Where(x => x.ReturnDate == null && x.DueDate < today);
        if (allowedLabIds != null)
            studentOverdueQuery = studentOverdueQuery
                .Where(x => allowedLabIds.Contains(x.LabId));

        var employeeOverdueQuery = _db.EmployeeIssuances
            .Where(x => x.ReturnDate == null && x.DueDate < today);
        if (allowedLabIds != null)
            employeeOverdueQuery = employeeOverdueQuery
                .Where(x => allowedLabIds.Contains(x.LabId));

        var overdueCount =
            await studentOverdueQuery.CountAsync() +
            await employeeOverdueQuery.CountAsync();

        var overall = new OverallStatsDto
        {
            TotalItems = items.Count,
            TotalIssued = items.Sum(i => i.IssuedQuantity),
            TotalRemaining = items.Sum(i => i.RemainingQuantity),
            TotalDefective = items.Sum(i => i.DefectiveQuantity),
            TotalLost = items.Sum(i => i.LostQuantity),
            OverdueCount = overdueCount
        };

        // ── Per-lab stats (only labs the user is assigned to) ──────
        var labQuery = _db.Labs.AsQueryable();
        if (allowedLabIds != null)
            labQuery = labQuery.Where(l => allowedLabIds.Contains(l.LabId));

        var labs = await labQuery.ToListAsync();

        var perLab = labs.Select(lab =>
        {
            var labItems = items.Where(i => i.LabId == lab.LabId).ToList();
            return new LabStatsDto
            {
                LabId = lab.LabId,
                LabName = lab.LabName,
                TotalItems = labItems.Count,
                IssuedItems = labItems.Sum(i => i.IssuedQuantity),
                RemainingItems = labItems.Sum(i => i.RemainingQuantity),
                DefectiveItems = labItems.Sum(i => i.DefectiveQuantity),
                LostItems = labItems.Sum(i => i.LostQuantity)
            };
        }).ToList();

        // ── Most issued items scoped to allowed labs ───────────────
        var studentIssuanceQuery = _db.StudentIssuances.Include(x => x.Item).AsQueryable();
        if (allowedLabIds != null)
            studentIssuanceQuery = studentIssuanceQuery
                .Where(x => allowedLabIds.Contains(x.LabId));

        var studentCounts = await studentIssuanceQuery
            .GroupBy(x => x.Item.EquipmentName)
            .Select(g => new { ItemName = g.Key, Count = g.Count() })
            .ToListAsync();

        var employeeIssuanceQuery = _db.EmployeeIssuances.Include(x => x.Item).AsQueryable();
        if (allowedLabIds != null)
            employeeIssuanceQuery = employeeIssuanceQuery
                .Where(x => allowedLabIds.Contains(x.LabId));

        var employeeCounts = await employeeIssuanceQuery
            .GroupBy(x => x.Item.EquipmentName)
            .Select(g => new { ItemName = g.Key, Count = g.Count() })
            .ToListAsync();

        var mostIssued = studentCounts
            .Concat(employeeCounts)
            .GroupBy(x => x.ItemName)
            .Select(g => new MostIssuedItemDto
            {
                ItemName = g.Key,
                TotalIssuances = g.Sum(x => x.Count)
            })
            .OrderByDescending(x => x.TotalIssuances)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto
        {
            Overall = overall,
            PerLab = perLab,
            MostIssuedItems = mostIssued
        };
    }
}