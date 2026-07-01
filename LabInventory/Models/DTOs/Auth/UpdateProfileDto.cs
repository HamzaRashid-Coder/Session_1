// Models/DTOs/Auth/UpdateProfileDto.cs
namespace LabInventory.Models.DTOs.Auth;

public class UpdateProfileDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string? PhoneNumber { get; set; }
}