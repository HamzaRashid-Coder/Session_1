namespace LabInventory.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string actionType, string tableName, string recordId,
                      string? oldValues = null, string? newValues = null, string? ipAddress = null);
    }
}
