using LabInventory.Models.Entities;

public class StudentIssuance
{
    public int StudentIssuanceId { get; set; }
    public int LabId { get; set; }
    public int ItemId { get; set; }
    public int QuantityIssued { get; set; }
    public string Student1Name { get; set; }
    public string RegistrationNo1 { get; set; }
    public string? ContactNo1 { get; set; }
    public string? Student2Name { get; set; }
    public string? RegistrationNo2 { get; set; }
    public string? ContactNo2 { get; set; }
    public string? Student3Name { get; set; }
    public string? RegistrationNo3 { get; set; }
    public string? DepartmentProgram { get; set; }
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
    public string? ProjectName { get; set; }
    public string? TeacherName { get; set; }
    public DateOnly? ExtendedDueDate { get; set; }
}