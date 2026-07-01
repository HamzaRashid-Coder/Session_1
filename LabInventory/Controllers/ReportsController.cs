using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Reports;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private bool IsAdmin() =>
        User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");

    private List<int> GetAssignedLabIds() =>
        User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

    [RequirePermission("reports.read")]
    [HttpGet("student-issuances")]
    public async Task<IActionResult> StudentIssuances([FromQuery] ReportFilterDto filter)
    {
        if (!IsAdmin())
        {
            var assigned = GetAssignedLabIds();
            if (filter.LabId.HasValue && !assigned.Contains(filter.LabId.Value))
                return Forbid();
            if (!filter.LabId.HasValue)
                filter.AllowedLabIds = assigned;
        }
        var result = await _reportService.GetStudentIssuancesAsync(filter);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("employee-issuances")]
    public async Task<IActionResult> EmployeeIssuances([FromQuery] ReportFilterDto filter)
    {
        if (!IsAdmin())
        {
            var assigned = GetAssignedLabIds();
            if (filter.LabId.HasValue && !assigned.Contains(filter.LabId.Value))
                return Forbid();
            if (!filter.LabId.HasValue)
                filter.AllowedLabIds = assigned;
        }
        var result = await _reportService.GetEmployeeIssuancesAsync(filter);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("overdue")]
    public async Task<IActionResult> Overdue()
    {
        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _reportService.GetOverdueAsync(allowedLabIds);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("fines")]
    public async Task<IActionResult> Fines()
    {
        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _reportService.GetFinesAsync(allowedLabIds);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("defective-lost")]
    public async Task<IActionResult> DefectiveLost()
    {
        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var result = await _reportService.GetDefectiveLostAsync(allowedLabIds);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("lab-inventory/{labId}")]
    public async Task<IActionResult> LabInventory(int labId)
    {
        if (!IsAdmin() && !GetAssignedLabIds().Contains(labId))
            return Forbid();

        var result = await _reportService.GetLabInventoryAsync(labId);
        return Ok(result);
    }

    [RequirePermission("reports.read")]
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] string reportType)
    {
        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var bytes = await _reportService.GenerateExcelAsync(reportType, allowedLabIds);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{reportType}-report.xlsx");
    }
}