namespace LabInventory.Models.DTOs.Issuance
{
    public class ReturnItemDto
    {
        public DateOnly ReturnDate { get; set; }
        public string Condition { get; set; }    // Good / Broken / Lost
        public int? ReturnCheckedByUserId { get; set; }
        public string? Remarks { get; set; }
    }
}
