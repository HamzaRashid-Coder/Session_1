// Services/Implementations/AuthService.cs
using LabInventory.Authentication.JWT;
using LabInventory.Data;
using LabInventory.Models.DTOs.Auth;
using LabInventory.Models.DTOs.Users;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(AppDbContext context, JwtTokenGenerator jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.RoleName)
            .ToListAsync();

        var permissionKeys = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.PermissionKey)
            .Distinct()
            .ToListAsync();

        var labIds = await _context.UserLabs
            .Where(ul => ul.UserId == user.UserId)
            .Select(ul => ul.LabId)
            .ToListAsync();

        return new LoginResponseDto
        {
            Token = _jwt.Generate(user, roles, permissionKeys, labIds)
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Check uniqueness only if the value changed
        if (user.Email != dto.Email)
        {
            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserId != userId);
            if (emailTaken)
                throw new InvalidOperationException("This email is already used by another account.");
        }

        if (user.Username != dto.Username)
        {
            bool usernameTaken = await _context.Users
                .AnyAsync(u => u.Username == dto.Username && u.UserId != userId);
            if (usernameTaken)
                throw new InvalidOperationException("This username is already taken.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Username = dto.Username;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}