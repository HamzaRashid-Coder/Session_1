using LabInventory.Data;
using LabInventory.Models.DTOs.Roles;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        return await _db.Roles
            .Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            })
            .ToListAsync();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            RoleName = dto.RoleName,
            CreatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return new RoleDto { RoleId = role.RoleId, RoleName = role.RoleName };
    }

    public async Task<RoleDto> UpdateRoleAsync(int roleId, CreateRoleDto dto)
    {
        var role = await _db.Roles.FindAsync(roleId)
            ?? throw new InvalidOperationException("Role not found.");

        role.RoleName = dto.RoleName;
        await _db.SaveChangesAsync();

        return new RoleDto { RoleId = role.RoleId, RoleName = role.RoleName };
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId)
            ?? throw new InvalidOperationException("Role not found.");

        bool hasUsers = await _db.UserRoles.AnyAsync(x => x.RoleId == roleId);
        if (hasUsers)
            throw new InvalidOperationException(
                "Cannot delete a role that is assigned to users.");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }

    public async Task AssignPermissionAsync(int roleId, int permissionId)
    {
        var exists = await _db.RolePermissions
            .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

        if (!exists)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemovePermissionAsync(int roleId, int permissionId)
    {
        var record = await _db.RolePermissions
            .FirstOrDefaultAsync(x =>
                x.RoleId == roleId && x.PermissionId == permissionId);

        if (record != null)
        {
            _db.RolePermissions.Remove(record);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<object>> GetRolePermissionsAsync(int roleId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => (object)new
            {
                rp.Permission.PermissionId,
                rp.Permission.PermissionKey,
                rp.Permission.PermissionName,
                rp.Permission.ModuleName
            })
            .ToListAsync();
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _db.Permissions.ToListAsync();
    }
}