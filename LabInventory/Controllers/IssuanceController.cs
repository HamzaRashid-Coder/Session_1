using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Issuance;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/issuances")]
[Authorize]
public class IssuanceController : ControllerBase
{
    private readonly IIssuanceService _issuanceService;

    public IssuanceController(IIssuanceService issuanceService)
    {
        _issuanceService = issuanceService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("userId")!);

    private bool IsAdmin() =>
        User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");

    private List<int> GetAssignedLabIds() =>
        User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

    // Validates that a requested labId is within the user's allowed scope.
    // Returns Forbid result if not allowed, null if allowed.
    private IActionResult? EnforceLabAccess(int? requestedLabId)
    {
        if (IsAdmin()) return null; // admins see everything

        var assignedLabIds = GetAssignedLabIds();

        if (requestedLabId.HasValue)
        {
            // Requested a specific lab — must be assigned to it
            if (!assignedLabIds.Contains(requestedLabId.Value))
                return Forbid();
        }
        else
        {
            // No specific lab requested but user has no assignments — deny
            if (!assignedLabIds.Any())
                return Forbid();
        }

        return null;
    }

    [RequirePermission("student_issuance.create")]
    [HttpPost("student")]
    public async Task<IActionResult> IssueToStudent([FromBody] CreateStudentIssuanceDto dto)
    {
        // Non-admins can only issue to their own labs
        var guard = EnforceLabAccess(dto.LabId);
        if (guard != null) return guard;

        var result = await _issuanceService.IssueToStudentAsync(dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(
            new { result.StudentIssuanceId },
            "Item issued to student successfully."));
    }

    [RequirePermission("employee_issuance.create")]
    [HttpPost("employee")]
    public async Task<IActionResult> IssueToEmployee([FromBody] CreateEmployeeIssuanceDto dto)
    {
        var guard = EnforceLabAccess(dto.LabId);
        if (guard != null) return guard;

        var result = await _issuanceService.IssueToEmployeeAsync(dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(
            new { result.EmployeeIssuanceId },
            "Item issued to employee successfully."));
    }

    [RequirePermission("student_issuance.update")]
    [HttpPost("student/{id}/return")]
    public async Task<IActionResult> ReturnStudent(int id, [FromBody] ReturnItemDto dto)
    {
        // Validate the issuance belongs to a lab the user can access
        if (!IsAdmin())
        {
            var issuance = await _issuanceService.GetStudentIssuanceByIdAsync(id);
            if (issuance == null)
                return NotFound(ApiResponse<object>.Fail("Issuance not found."));

            // Cast to dynamic to read LabId — service returns anonymous object
            var labId = (int)((dynamic)issuance).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        await _issuanceService.ReturnStudentIssuanceAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Student item returned successfully."));
    }

    [RequirePermission("employee_issuance.update")]
    [HttpPost("employee/{id}/return")]
    public async Task<IActionResult> ReturnEmployee(int id, [FromBody] ReturnItemDto dto)
    {
        if (!IsAdmin())
        {
            var issuance = await _issuanceService.GetEmployeeIssuanceByIdAsync(id);
            if (issuance == null)
                return NotFound(ApiResponse<object>.Fail("Issuance not found."));

            var labId = (int)((dynamic)issuance).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        await _issuanceService.ReturnEmployeeIssuanceAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Employee item returned successfully."));
    }

    [RequirePermission("student_issuance.read")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] int? labId)
    {
        if (!IsAdmin())
        {
            var assignedLabIds = GetAssignedLabIds();

            if (labId.HasValue)
            {
                // Requested a specific lab — must be assigned
                if (!assignedLabIds.Contains(labId.Value))
                    return Forbid();
            }
            else
            {
                // No filter supplied — return only their labs by forcing the first assigned lab
                // The service will filter; if they have multiple labs the frontend should pass labId
                if (!assignedLabIds.Any())
                    return Forbid();

                // If non-admin has exactly one lab, auto-scope it
                if (assignedLabIds.Count == 1)
                    labId = assignedLabIds[0];
                // If they have multiple assigned labs and no filter, still restrict via service
            }
        }

        var result = await _issuanceService.GetActiveIssuancesAsync(labId, IsAdmin() ? null : GetAssignedLabIds());
        return Ok(ApiResponse<List<object>>.Ok(result));
    }

    [RequirePermission("student_issuance.read")]
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    {
        var assignedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _issuanceService.GetOverdueIssuancesAsync(assignedLabIds);
        return Ok(ApiResponse<List<object>>.Ok(result));
    }

    [RequirePermission("student_issuance.read")]
    [HttpGet("student/{id}")]
    public async Task<IActionResult> GetStudentIssuance(int id)
    {
        var result = await _issuanceService.GetStudentIssuanceByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail("Issuance not found."));

        // Non-admin can only view issuances from their labs
        if (!IsAdmin())
        {
            var labId = (int)((dynamic)result).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        return Ok(ApiResponse<object>.Ok(result));
    }

    [RequirePermission("employee_issuance.read")]
    [HttpGet("employee/{id}")]
    public async Task<IActionResult> GetEmployeeIssuance(int id)
    {
        var result = await _issuanceService.GetEmployeeIssuanceByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail("Issuance not found."));

        if (!IsAdmin())
        {
            var labId = (int)((dynamic)result).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        return Ok(ApiResponse<object>.Ok(result));
    }

    [RequirePermission("student_issuance.create")]
    [HttpPost("student/bulk")]
    public async Task<IActionResult> BulkIssueToStudent([FromBody] BulkStudentIssuanceDto dto)
    {
        // Each item in the bulk request must belong to an allowed lab
        if (!IsAdmin())
        {
            var assignedLabIds = GetAssignedLabIds();
            var forbiddenLab = dto.Items.FirstOrDefault(i => !assignedLabIds.Contains(i.LabId));
            if (forbiddenLab != null)
                return Forbid();
        }

        var result = await _issuanceService.BulkIssueToStudentAsync(dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(
            new { count = result },
            $"{result} items issued successfully."));
    }

    [RequirePermission("student_issuance.read")]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int? labId,
        [FromQuery] string? issuedTo,
        [FromQuery] string? issuedBy,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (!IsAdmin())
        {
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _issuanceService.GetIssuanceHistoryAsync(
            labId, issuedTo, issuedBy, fromDate, toDate,
            allowedLabIds, page, pageSize);

        return Ok(ApiResponse<object>.Ok(result));
    }
    [RequirePermission("student_issuance.update")]
    [HttpPost("student/{id}/extend-due-date")]
    public async Task<IActionResult> ExtendStudentDueDate(int id, [FromBody] ExtendDueDateDto dto)
    {
        if (!IsAdmin())
        {
            var issuance = await _issuanceService.GetStudentIssuanceByIdAsync(id);
            if (issuance == null)
                return NotFound(ApiResponse<object>.Fail("Issuance not found."));

            var labId = (int)((dynamic)issuance).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        await _issuanceService.ExtendStudentDueDateAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Due date extended successfully."));
    }

    [RequirePermission("employee_issuance.update")]
    [HttpPost("employee/{id}/extend-due-date")]
    public async Task<IActionResult> ExtendEmployeeDueDate(int id, [FromBody] ExtendDueDateDto dto)
    {
        if (!IsAdmin())
        {
            var issuance = await _issuanceService.GetEmployeeIssuanceByIdAsync(id);
            if (issuance == null)
                return NotFound(ApiResponse<object>.Fail("Issuance not found."));

            var labId = (int)((dynamic)issuance).LabId;
            var guard = EnforceLabAccess(labId);
            if (guard != null) return guard;
        }

        await _issuanceService.ExtendEmployeeDueDateAsync(id, dto, GetUserId());
        return Ok(ApiResponse<object>.Ok(null, "Due date extended successfully."));
    }
}