namespace LabInventory.Models.DTOs.Issuance
{
    public class ExtendDueDateDto
    {
        public DateOnly NewDueDate { get; set; }
        public string? Remarks { get; set; }
    }
}