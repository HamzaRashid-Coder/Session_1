// ── IFineService.cs ──────────────────────────────────────────────────────────
using LabInventory.Models.DTOs.Fines;

namespace LabInventory.Services.Interfaces;

public interface IFineService
{
    /// <summary>
    /// Returns all issuances that have a fine (FineAmount > 0),
    /// split into paid and unpaid, optionally filtered by lab.
    /// </summary>
    Task<FinesSummaryDto> GetFinesSummaryAsync(int? labId, List<int>? allowedLabIds);

    /// <summary>
    /// Marks a fine as paid for a student issuance.
    /// </summary>
    Task MarkStudentFinePaidAsync(int issuanceId, MarkFinePaidDto dto, int collectedByUserId);

    /// <summary>
    /// Marks a fine as paid for an employee issuance.
    /// </summary>
    Task MarkEmployeeFinePaidAsync(int issuanceId, MarkFinePaidDto dto, int collectedByUserId);
}


