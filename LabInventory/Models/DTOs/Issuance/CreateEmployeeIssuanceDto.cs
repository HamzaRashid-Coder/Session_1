namespace LabInventory.Models.DTOs.Issuance
{
    public class CreateEmployeeIssuanceDto
    {
        public int LabId { get; set; }
        public int ItemId { get; set; }
        public string FacultyName { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? Department { get; set; }
        public int QuantityIssued { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? Remarks { get; set; }
    }
}
