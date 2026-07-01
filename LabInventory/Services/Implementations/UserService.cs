using LabInventory.Data;
using LabInventory.Models.DTOs.Users;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _db.Users.Select(x => new UserDto
        {
            UserId = x.UserId,
            FullName = x.FullName,
            Email = x.Email,
            Username = x.Username,
            PhoneNumber = x.PhoneNumber,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            Roles = _db.UserRoles
                .Where(ur => ur.UserId == x.UserId)
                .Select(ur => new UserRoleInfo
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.RoleName
                }).ToList(),
            Labs = _db.UserLabs
                .Where(ul => ul.UserId == x.UserId)
                .Select(ul => new UserLabInfo
                {
                    LabId = ul.LabId,
                    LabName = ul.Lab.LabName
                }).ToList()
        }).ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        return await _db.Users
            .Where(x => x.UserId == userId)
            .Select(x => new UserDto
            {
                UserId = x.UserId,
                FullName = x.FullName,
                Email = x.Email,
                Username = x.Username,
                PhoneNumber = x.PhoneNumber,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                Roles = _db.UserRoles
                    .Where(ur => ur.UserId == x.UserId)
                    .Select(ur => new UserRoleInfo
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.RoleName
                    }).ToList(),
                Labs = _db.UserLabs
                    .Where(ul => ul.UserId == x.UserId)
                    .Select(ul => new UserLabInfo
                    {
                        LabId = ul.LabId,
                        LabName = ul.Lab.LabName
                    }).ToList()
            }).FirstOrDefaultAsync();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            throw new InvalidOperationException("A user with this email already exists.");

        bool usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (usernameExists)
            throw new InvalidOperationException("This username is already taken.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Username = dto.Username,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Username = user.Username,
            IsActive = user.IsActive
        };
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Username = dto.Username;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found after update.");
    }

    public async Task AssignRoleAsync(int userId, int roleId)
    {
        bool alreadyAssigned = await _db.UserRoles
            .AnyAsync(x => x.UserId == userId && x.RoleId == roleId);

        if (alreadyAssigned)
            throw new InvalidOperationException("This role is already assigned to the user.");

        _db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(int userId, int roleId)
    {
        var record = await _db.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);

        if (record != null)
        {
            _db.UserRoles.Remove(record);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AssignLabAsync(int userId, int labId)
    {
        bool alreadyAssigned = await _db.UserLabs
            .AnyAsync(x => x.UserId == userId && x.LabId == labId);

        if (alreadyAssigned)
            throw new InvalidOperationException("This lab is already assigned to the user.");

        _db.UserLabs.Add(new UserLab
        {
            UserId = userId,
            LabId = labId,
            AssignedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveLabAsync(int userId, int labId)
    {
        var record = await _db.UserLabs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LabId == labId);

        if (record != null)
        {
            _db.UserLabs.Remove(record);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeactivateUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
    public async Task ReactivateUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (user.IsActive)
            throw new InvalidOperationException("User is already active.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
    public async Task ResetPasswordAsync(int userId, AdminResetPasswordDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            throw new InvalidOperationException("New password must be at least 6 characters.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}