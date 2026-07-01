using LabInventory.Models.DTOs.Users;


namespace LabInventory.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task DeactivateUserAsync(int userId);
    Task AssignRoleAsync(int userId, int roleId);
    Task RemoveRoleAsync(int userId, int roleId);
    Task AssignLabAsync(int userId, int labId);
    Task RemoveLabAsync(int userId, int labId);
    Task ReactivateUserAsync(int userId);
    Task ResetPasswordAsync(int userId, AdminResetPasswordDto dto);
}