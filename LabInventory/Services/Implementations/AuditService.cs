using LabInventory.Data;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations
{
    
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;

        public AuditService(AppDbContext db) => _db = db;

        public async Task LogAsync(int? userId, string actionType, string tableName,
                                   string recordId, string? oldValues = null,
                                   string? newValues = null, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                ActionType = actionType,
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues,
                NewValues = newValues,
                IPAddress = ipAddress,
                ActionTimestamp = DateTime.UtcNow
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
