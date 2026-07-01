using LabInventory.Models.DTOs.Dashboard;

namespace LabInventory.Services.Interfaces
{
    public interface IDashboardService
    {
        // allowedLabIds = null → admin sees everything
        // allowedLabIds = [1,2] → non-admin sees only those labs
        Task<DashboardSummaryDto> GetSummaryAsync(int userId, bool isAdmin, List<int>? allowedLabIds = null);
    }
}