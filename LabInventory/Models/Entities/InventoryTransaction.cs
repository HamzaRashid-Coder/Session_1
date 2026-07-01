namespace LabInventory.Models.Entities
{
    public class InventoryTransaction
    {
        public long TransactionId { get; set; }
        public int ItemId { get; set; }
        public string TransactionType { get; set; }   // ISSUE / RETURN_GOOD / RETURN_DEFECTIVE / RETURN_LOST
        public int Quantity { get; set; }
        public string? ReferenceType { get; set; }    // STUDENT_ISSUANCE / EMPLOYEE_ISSUANCE
        public int? ReferenceId { get; set; }
        public int PerformedBy { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }

        public InventoryItem Item { get; set; }
        public User PerformedByUser { get; set; }
    }
}
