using LabInventory.Data;
using LabInventory.Models.DTOs.Fines;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class FineService : IFineService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public FineService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<FinesSummaryDto> GetFinesSummaryAsync(
        int? labId, List<int>? allowedLabIds)
    {
        // ── Student fines ────────────────────────────────────────────
        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.FinePaidByUser)
            .Where(x => x.FineAmount > 0);

        if (labId.HasValue)
            studentQuery = studentQuery.Where(x => x.LabId == labId.Value);
        else if (allowedLabIds != null)
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var studentFines = await studentQuery.Select(x => new FineRecordDto
        {
            Id = x.StudentIssuanceId,
            Type = "Student",
            Name = x.Student1Name,
            RegistrationNo = x.RegistrationNo1,
            Item = x.Item.EquipmentName,
            Lab = x.Lab.LabName,
            LabId = x.LabId,
            ReturnDate = x.ReturnDate,
            ConditionOnReturn = x.ConditionOnReturn,
            FineAmount = x.FineAmount,
            FinePaidAmount = x.FinePaidAmount,
            FinePaidAt = x.FinePaidAt,
            FinePaidByName = x.FinePaidByUser != null ? x.FinePaidByUser.FullName : null,
            IsPaid = x.FinePaidAt != null
        }).ToListAsync();

        // ── Employee fines ───────────────────────────────────────────
        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.FinePaidByUser)
            .Where(x => x.FineAmount > 0);

        if (labId.HasValue)
            employeeQuery = employeeQuery.Where(x => x.LabId == labId.Value);
        else if (allowedLabIds != null)
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var employeeFines = await employeeQuery.Select(x => new FineRecordDto
        {
            Id = x.EmployeeIssuanceId,
            Type = "Employee",
            Name = x.FacultyName,
            RegistrationNo = null,
            Item = x.Item.EquipmentName,
            Lab = x.Lab.LabName,
            LabId = x.LabId,
            ReturnDate = x.ReturnDate,
            ConditionOnReturn = x.ConditionOnReturn,
            FineAmount = x.FineAmount,
            FinePaidAmount = x.FinePaidAmount,
            FinePaidAt = x.FinePaidAt,
            FinePaidByName = x.FinePaidByUser != null ? x.FinePaidByUser.FullName : null,
            IsPaid = x.FinePaidAt != null
        }).ToListAsync();

        var all = studentFines.Concat(employeeFines).ToList();

        return new FinesSummaryDto
        {
            Unpaid = all.Where(x => !x.IsPaid).OrderBy(x => x.ReturnDate).ToList(),
            Paid = all.Where(x => x.IsPaid).OrderByDescending(x => x.FinePaidAt).ToList(),
            TotalUnpaid = all.Where(x => !x.IsPaid).Sum(x => x.FineAmount),
            TotalPaid = all.Where(x => x.IsPaid).Sum(x => x.FinePaidAmount ?? 0),
            TotalAssessed = all.Sum(x => x.FineAmount)
        };
    }

    public async Task MarkStudentFinePaidAsync(
        int issuanceId, MarkFinePaidDto dto, int collectedByUserId)
    {
        var issuance = await _db.StudentIssuances.FindAsync(issuanceId)
            ?? throw new InvalidOperationException("Student issuance not found.");

        if (issuance.FineAmount <= 0)
            throw new InvalidOperationException("This issuance has no fine.");

        if (issuance.FinePaidAt != null)
            throw new InvalidOperationException("Fine already marked as paid.");

        issuance.FinePaidAmount = dto.AmountPaid;
        issuance.FinePaidAt = DateTime.UtcNow;
        issuance.FinePaidBy = collectedByUserId;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(collectedByUserId, "FinePaid", "StudentIssuances",
            issuanceId.ToString(),
            newValues: $"{{AmountPaid: {dto.AmountPaid}, CollectedBy: {collectedByUserId}}}");
    }

    public async Task MarkEmployeeFinePaidAsync(
        int issuanceId, MarkFinePaidDto dto, int collectedByUserId)
    {
        var issuance = await _db.EmployeeIssuances.FindAsync(issuanceId)
            ?? throw new InvalidOperationException("Employee issuance not found.");

        if (issuance.FineAmount <= 0)
            throw new InvalidOperationException("This issuance has no fine.");

        if (issuance.FinePaidAt != null)
            throw new InvalidOperationException("Fine already marked as paid.");

        issuance.FinePaidAmount = dto.AmountPaid;
        issuance.FinePaidAt = DateTime.UtcNow;
        issuance.FinePaidBy = collectedByUserId;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(collectedByUserId, "FinePaid", "EmployeeIssuances",
            issuanceId.ToString(),
            newValues: $"{{AmountPaid: {dto.AmountPaid}, CollectedBy: {collectedByUserId}}}");
    }
}