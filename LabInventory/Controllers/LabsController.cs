using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Labs;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/labs")]
[Authorize]
public class LabsController : ControllerBase
{
    private readonly ILabService _labService;

    public LabsController(ILabService labService) => _labService = labService;

    private int GetUserId() =>
        int.Parse(User.FindFirstValue("userId")!);

    private bool IsAdmin() =>
        User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");

    private List<int> GetLabIds() =>
        User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

    [RequirePermission("labs.read")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var labs = await _labService.GetAllAsync(GetUserId(), IsAdmin());
        return Ok(ApiResponse<List<LabDto>>.Ok(labs));
    }

    [RequirePermission("labs.read")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lab = await _labService.GetByIdAsync(id);
        if (lab == null) return NotFound(ApiResponse<LabDto>.Fail("Lab not found."));
        return Ok(ApiResponse<LabDto>.Ok(lab));
    }

    [RequirePermission("labs.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabDto dto)
    {
        var lab = await _labService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = lab.LabId },
            ApiResponse<LabDto>.Ok(lab, "Lab created successfully."));
    }

    [RequirePermission("labs.update")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLabDto dto)
    {
        var lab = await _labService.UpdateAsync(id, dto);
        return Ok(ApiResponse<LabDto>.Ok(lab, "Lab updated successfully."));
    }

    [RequirePermission("labs.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _labService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "Lab deleted successfully."));
    }

    [RequirePermission("labs.read")]
    [HttpGet("{id}/users")]
    public async Task<IActionResult> GetLabUsers(int id)
    {
        var users = await _labService.GetLabUsersAsync(id);
        return Ok(ApiResponse<object>.Ok(users));
    }
}