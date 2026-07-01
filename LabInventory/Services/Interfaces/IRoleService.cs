using LabInventory.Models.DTOs.Roles;
using LabInventory.Models.Entities;

namespace LabInventory.Services.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateRoleAsync(int roleId, CreateRoleDto dto);
    Task DeleteRoleAsync(int roleId);
    Task AssignPermissionAsync(int roleId, int permissionId);
    Task RemovePermissionAsync(int roleId, int permissionId);
    Task<List<object>> GetRolePermissionsAsync(int roleId);
    Task<List<Permission>> GetAllPermissionsAsync();
}