using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Dashboard;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("userId")!);

    private bool IsAdmin() =>
        User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");

    private List<int> GetAssignedLabIds() =>
        User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

    [RequirePermission("dashboard.read")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        // Pass null for admins (no restriction), assigned lab IDs for everyone else
        var allowedLabIds = IsAdmin() ? null : GetAssignedLabIds();
        var summary = await _dashboardService.GetSummaryAsync(GetUserId(), IsAdmin(), allowedLabIds);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }
}