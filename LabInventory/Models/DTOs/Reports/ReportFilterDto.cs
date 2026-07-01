namespace LabInventory.Models.DTOs.Reports
{
    public class ReportFilterDto
    {
        public int? LabId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? SearchName { get; set; }
        public string? RegistrationNo { get; set; }

        // Set by controller only — never from query string
        [System.Text.Json.Serialization.JsonIgnore]
        public List<int>? AllowedLabIds { get; set; }
    }
}