namespace LabInventory.Models.Entities
{
    public class InventoryItem
    {
        public int ItemId { get; set; }
        public int LabId { get; set; }
        public string EquipmentName { get; set; }
        public string? ModelNumber { get; set; }
        public string? Placement { get; set; }
        public decimal FinePerDay { get; set; }
        public int TotalQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public int DefectiveQuantity { get; set; }
        public int LostQuantity { get; set; }
        public int RemainingQuantity { get; set; }   // computed by DB, read-only
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Lab Lab { get; set; }
        public User CreatedByUser { get; set; }
        public ICollection<StudentIssuance> StudentIssuances { get; set; }
        public ICollection<EmployeeIssuance> EmployeeIssuances { get; set; }
    }
}