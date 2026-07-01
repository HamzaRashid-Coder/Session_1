using LabInventory.Models.DTOs.Auth;
using LabInventory.Models.DTOs.Users;

namespace LabInventory.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}