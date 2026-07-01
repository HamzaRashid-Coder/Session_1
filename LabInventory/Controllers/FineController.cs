using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Fines;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/fines")]
[Authorize]
public class FineController : ControllerBase
{
    private readonly IFineService _fineService;

    public FineController(IFineService fineService)
    {
        _fineService = fineService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("userId")!);

    private bool IsAdmin() =>
        User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");

    private List<int> GetAssignedLabIds() =>
        User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

    private IActionResult? EnforceLabAccess(int? requestedLabId)
    {
        if (IsAdmin()) return null;
        var assigned = GetAssignedLabIds();
        if (requestedLabId.HasValue)
            return assigned.Contains(requestedLabId.Value) ? null : Forbid();
        return assigned.Any() ? null : Forbid();
    }

    /// <summary>
    /// GET /api/fines?labId=1
    /// Returns all fines split into paid / unpaid with summary totals.
    /// </summary>
    [RequirePermission("fines.manage")]
    [HttpGet]
    public async Task<IActionResult> GetFines([FromQuery] int? labId)
    {
        if (!IsAdmin())
        {
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _fineService.GetFinesSummaryAsync(labId, allowedLabIds);
        return Ok(ApiResponse<FinesSummaryDto>.Ok(result));
    }

    /// <summary>
    /// POST /api/fines/student/{id}/pay
    /// Mark a student issuance fine as paid.
    /// </summary>
    [RequirePermission("fines.manage")]
    [HttpPost("student/{id}/pay")]
    public async Task<IActionResult> PayStudentFine(int id, [FromBody] MarkFinePaidDto dto)
    {
        await _fineService.MarkStudentFinePaidAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Fine marked as paid."));
    }

    /// <summary>
    /// POST /api/fines/employee/{id}/pay
    /// Mark an employee issuance fine as paid.
    /// </summary>
    [RequirePermission("fines.manage")]
    [HttpPost("employee/{id}/pay")]
    public async Task<IActionResult> PayEmployeeFine(int id, [FromBody] MarkFinePaidDto dto)
    {
        await _fineService.MarkEmployeeFinePaidAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Fine marked as paid."));
    }
}