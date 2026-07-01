using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Inventory;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
        => _inventoryService = inventoryService;

    private int GetUserId() => int.Parse(User.FindFirstValue("userId")!);

    [RequirePermission("inventory.read")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? labId, [FromQuery] string? search)
    {
        var isAdmin = User.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin");
        var userLabIds = User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

        // Non-admin requesting a specific lab they aren't assigned to
        if (!isAdmin && labId.HasValue && !userLabIds.Contains(labId.Value))
            return Forbid();

        // Non-admin with no labId filter — restrict to their assigned labs
        var effectiveLabId = labId;
        List<int>? allowedLabIds = null;
        if (!isAdmin && !labId.HasValue)
            allowedLabIds = userLabIds;

        var items = await _inventoryService.GetAllAsync(effectiveLabId, search, allowedLabIds);
        return Ok(ApiResponse<List<InventoryItemDto>>.Ok(items));
    }

    [RequirePermission("inventory.read")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _inventoryService.GetByIdAsync(id);
        if (item == null) return NotFound(ApiResponse<InventoryItemDto>.Fail("Item not found."));
        return Ok(ApiResponse<InventoryItemDto>.Ok(item));
    }

    [RequirePermission("inventory.read")]
    [HttpGet("{id}/issuance-history")]
    public async Task<IActionResult> GetIssuanceHistory(int id)
    {
        var history = await _inventoryService.GetIssuanceHistoryAsync(id);
        return Ok(ApiResponse<object>.Ok(history));
    }

    [RequirePermission("inventory.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto dto)
    {
        var item = await _inventoryService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = item.ItemId },
            ApiResponse<InventoryItemDto>.Ok(item, "Item created successfully."));
    }

    [RequirePermission("inventory.update")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateInventoryItemDto dto)
    {
        var item = await _inventoryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<InventoryItemDto>.Ok(item, "Item updated successfully."));
    }

    
}