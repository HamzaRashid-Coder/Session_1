namespace LabInventory.Models.DTOs.Labs
{
    public class LabDto
    {
        public int LabId { get; set; }
        public string LabName { get; set; }
        public string? Location { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
