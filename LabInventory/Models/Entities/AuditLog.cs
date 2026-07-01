namespace LabInventory.Models.Entities
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IPAddress { get; set; }
        public DateTime ActionTimestamp { get; set; }

        public User? User { get; set; }
    }
}
