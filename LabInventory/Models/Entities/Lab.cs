namespace LabInventory.Models.Entities;

public class Lab
{
    public int LabId { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User CreatedByUser { get; set; } = null!;
    public ICollection<UserLab> UserLabs { get; set; } = new List<UserLab>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}