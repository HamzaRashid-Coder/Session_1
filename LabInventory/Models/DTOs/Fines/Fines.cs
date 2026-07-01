// ── DTOs/Fines/ ──────────────────────────────────────────────────────────────
namespace LabInventory.Models.DTOs.Fines;

public class MarkFinePaidDto
{
    /// <summary>Amount actually collected (may differ from assessed fine).</summary>
    public decimal AmountPaid { get; set; }
}

public class FineRecordDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";        // "Student" | "Employee"
    public string Name { get; set; } = "";        // student1Name or facultyName
    public string? RegistrationNo { get; set; }
    public string Item { get; set; } = "";
    public string Lab { get; set; } = "";
    public int LabId { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string? ConditionOnReturn { get; set; }
    public decimal FineAmount { get; set; }
    public decimal? FinePaidAmount { get; set; }
    public DateTime? FinePaidAt { get; set; }
    public string? FinePaidByName { get; set; }
    public bool IsPaid { get; set; }
}

public class FinesSummaryDto
{
    public List<FineRecordDto> Unpaid { get; set; } = new();
    public List<FineRecordDto> Paid { get; set; } = new();
    public decimal TotalUnpaid { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalAssessed { get; set; }
}