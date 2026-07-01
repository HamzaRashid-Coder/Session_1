// Controllers/AuthController.cs
using LabInventory.Helpers;
using LabInventory.Models.DTOs.Auth;
using LabInventory.Models.DTOs.Users;
using LabInventory.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue("userId");
        var email = User.FindFirstValue("email");
        var name = User.FindFirstValue(ClaimTypes.Name);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
        var labIds = User.FindAll("labIds").Select(c => int.Parse(c.Value)).ToList();

        return Ok(new
        {
            userId,
            email,
            name,
            roles,
            permissions,
            labIds
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        await _auth.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(null, "Password changed successfully."));
    }

    /// <summary>
    /// PUT /api/auth/profile
    /// Authenticated user updates their own name, email, username, phone.
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = int.Parse(User.FindFirstValue("userId")!);
        var updated = await _auth.UpdateProfileAsync(userId, dto);
        return Ok(ApiResponse<UserDto>.Ok(updated, "Profile updated successfully."));
    }
}