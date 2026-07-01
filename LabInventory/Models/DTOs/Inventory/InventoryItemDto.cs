namespace LabInventory.Models.DTOs.Inventory
{
    public class InventoryItemDto
    {
        public int ItemId { get; set; }
        public int LabId { get; set; }
        public string LabName { get; set; }
        public string EquipmentName { get; set; }
        public string? ModelNumber { get; set; }
        public string? Placement { get; set; }
        public decimal FinePerDay { get; set; }
        public int TotalQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public int DefectiveQuantity { get; set; }
        public int LostQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
