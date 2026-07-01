namespace LabInventory.Models.DTOs.Users;

public class UserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserRoleInfo> Roles { get; set; } = new();
    public List<UserLabInfo> Labs { get; set; } = new();
}

public class UserRoleInfo
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class UserLabInfo
{
    public int LabId { get; set; }
    public string LabName { get; set; } = string.Empty;
}