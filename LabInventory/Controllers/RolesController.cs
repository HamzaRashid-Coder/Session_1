using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Roles;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [RequirePermission("roles.manage")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
    }

    [RequirePermission("roles.manage")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateRoleAsync(dto);
        return Ok(ApiResponse<RoleDto>.Ok(role, "Role created."));
    }

    [RequirePermission("roles.manage")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.UpdateRoleAsync(id, dto);
        return Ok(ApiResponse<RoleDto>.Ok(role, "Role updated."));
    }

    [RequirePermission("roles.manage")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roleService.DeleteRoleAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "Role deleted."));
    }

    [RequirePermission("roles.manage")]
    [HttpPost("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AssignPermission(int roleId, int permissionId)
    {
        await _roleService.AssignPermissionAsync(roleId, permissionId);
        return Ok(ApiResponse<object>.Ok(null, "Permission assigned."));
    }

    [RequirePermission("roles.manage")]
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermission(int roleId, int permissionId)
    {
        await _roleService.RemovePermissionAsync(roleId, permissionId);
        return Ok(ApiResponse<object>.Ok(null, "Permission removed."));
    }

    [RequirePermission("roles.manage")]
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        return Ok(ApiResponse<object>.Ok(permissions));
    }

    [RequirePermission("roles.manage")]
    [HttpGet("permissions/all")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _roleService.GetAllPermissionsAsync();
        return Ok(ApiResponse<object>.Ok(permissions));
    }
}