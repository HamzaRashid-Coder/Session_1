using LabInventory.Models.Entities;

public class EmployeeIssuance
{
    public int EmployeeIssuanceId { get; set; }
    public int LabId { get; set; }
    public int ItemId { get; set; }
    public string FacultyName { get; set; }
    public string? Email { get; set; }
    public string? ContactNo { get; set; }
    public string? Department { get; set; }
    public int QuantityIssued { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string? ConditionOnReturn { get; set; }
    public decimal FineAmount { get; set; }
    public int IssuedBy { get; set; }
    public int? ReturnCheckedBy { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── Fine payment tracking ────────────────────────────
    public decimal? FinePaidAmount { get; set; }
    public DateTime? FinePaidAt { get; set; }
    public int? FinePaidBy { get; set; }

    // ── Computed helpers ─────────────────────────────────
    public bool IsReturned => ReturnDate != null;
    public bool IsFinePaid => FinePaidAt != null;

    // ── Navigation properties ────────────────────────────
    public Lab Lab { get; set; }
    public InventoryItem Item { get; set; }
    public User IssuedByUser { get; set; }
    public User? ReturnCheckedByUser { get; set; }
    public User? FinePaidByUser { get; set; }
    public DateOnly? ExtendedDueDate { get; set; }
}