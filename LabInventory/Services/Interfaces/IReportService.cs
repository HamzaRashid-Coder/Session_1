using LabInventory.Models.DTOs.Reports;

namespace LabInventory.Services.Interfaces;

public interface IReportService
{
    Task<List<object>> GetStudentIssuancesAsync(ReportFilterDto filter);
    Task<List<object>> GetEmployeeIssuancesAsync(ReportFilterDto filter);
    Task<List<object>> GetOverdueAsync(List<int>? allowedLabIds);
    Task<List<object>> GetFinesAsync(List<int>? allowedLabIds);
    Task<List<object>> GetDefectiveLostAsync(List<int>? allowedLabIds);
    Task<List<object>> GetLabInventoryAsync(int labId);
    Task<byte[]> GenerateExcelAsync(string reportType, List<int>? allowedLabIds);
}