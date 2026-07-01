using LabInventory.Authentication.Authorization;
using LabInventory.Data;
using LabInventory.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditLogsController(AppDbContext db) => _db = db;

    [RequirePermission("roles.manage")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? tableName,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.ActionType == action);

        if (!string.IsNullOrWhiteSpace(tableName))
            query = query.Where(x => x.TableName == tableName);

        if (fromDate.HasValue)
            query = query.Where(x => x.ActionTimestamp >= fromDate);

        if (toDate.HasValue)
            query = query.Where(x => x.ActionTimestamp <= toDate);

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(x => x.ActionTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.AuditLogId,
                x.UserId,
                x.ActionType,
                x.TableName,
                x.RecordId,
                x.OldValues,
                x.NewValues,
                x.IPAddress,
                x.ActionTimestamp
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { total, page, pageSize, logs }));
    }
}