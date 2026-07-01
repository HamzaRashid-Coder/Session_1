// Controllers/UsersController.cs
using LabInventory.Authentication.Authorization;
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Users;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [RequirePermission("users.manage")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    [RequirePermission("users.manage")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [RequirePermission("users.manage")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return Ok(ApiResponse<UserDto>.Ok(user, "User created."));
    }

    [RequirePermission("users.manage")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(ApiResponse<UserDto>.Ok(user, "User updated."));
    }

    [RequirePermission("users.manage")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _userService.DeactivateUserAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "User deactivated."));
    }

    [RequirePermission("users.manage")]
    [HttpPatch("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        await _userService.ReactivateUserAsync(id);
        return Ok(ApiResponse<object>.Ok(null, "User reactivated."));
    }

    [RequirePermission("users.manage")]
    [HttpPost("{id}/roles/{roleId}")]
    public async Task<IActionResult> AssignRole(int id, int roleId)
    {
        await _userService.AssignRoleAsync(id, roleId);
        return Ok(ApiResponse<object>.Ok(null, "Role assigned."));
    }

    [RequirePermission("users.manage")]
    [HttpDelete("{id}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRole(int id, int roleId)
    {
        await _userService.RemoveRoleAsync(id, roleId);
        return Ok(ApiResponse<object>.Ok(null, "Role removed."));
    }

    [RequirePermission("users.manage")]
    [HttpPost("{id}/labs/{labId}")]
    public async Task<IActionResult> AssignLab(int id, int labId)
    {
        await _userService.AssignLabAsync(id, labId);
        return Ok(ApiResponse<object>.Ok(null, "Lab assigned."));
    }

    [RequirePermission("users.manage")]
    [HttpDelete("{id}/labs/{labId}")]
    public async Task<IActionResult> RemoveLab(int id, int labId)
    {
        await _userService.RemoveLabAsync(id, labId);
        return Ok(ApiResponse<object>.Ok(null, "Lab removed."));
    }

    /// <summary>
    /// POST /api/users/{id}/reset-password
    /// Admin resets a user's password without needing the old one.
    /// </summary>
    [RequirePermission("users.manage")]
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] AdminResetPasswordDto dto)
    {
        await _userService.ResetPasswordAsync(id, dto);
        return Ok(ApiResponse<object>.Ok(null, "Password reset successfully."));
    }
}