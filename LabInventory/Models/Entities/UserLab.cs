namespace LabInventory.Models.Entities
{
    public class UserLab
    {
        public int UserLabId { get; set; }
        public int UserId { get; set; }
        public int LabId { get; set; }
        public DateTime AssignedAt { get; set; }

        public User User { get; set; }
        public Lab Lab { get; set; }
    }
}
