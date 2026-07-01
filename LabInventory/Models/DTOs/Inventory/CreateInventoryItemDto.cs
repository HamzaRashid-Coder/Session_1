namespace LabInventory.Models.DTOs.Inventory
{
    public class CreateInventoryItemDto
    {
        public int LabId { get; set; }
        public string EquipmentName { get; set; }
        public string? ModelNumber { get; set; }
        public string? Placement { get; set; }
        public decimal FinePerDay { get; set; }
        public int TotalQuantity { get; set; }
    }
}
