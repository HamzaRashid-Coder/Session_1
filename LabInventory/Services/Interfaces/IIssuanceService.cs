using LabInventory.Models.DTOs.Issuance;

public interface IIssuanceService
{
    Task<StudentIssuance> IssueToStudentAsync(CreateStudentIssuanceDto dto, int userId);
    Task<EmployeeIssuance> IssueToEmployeeAsync(CreateEmployeeIssuanceDto dto, int userId);
    Task ReturnStudentIssuanceAsync(int id, ReturnItemDto dto, int userId);
    Task ReturnEmployeeIssuanceAsync(int id, ReturnItemDto dto, int userId);
    // allowedLabIds = null means no restriction (admin); empty list means no access
    Task<List<object>> GetActiveIssuancesAsync(int? labId, List<int>? allowedLabIds = null);
    Task<List<object>> GetOverdueIssuancesAsync(List<int>? allowedLabIds = null);
    Task<object?> GetStudentIssuanceByIdAsync(int id);
    Task<object?> GetEmployeeIssuanceByIdAsync(int id);
    Task<int> BulkIssueToStudentAsync(BulkStudentIssuanceDto dto, int userId);
    Task<object> GetIssuanceHistoryAsync(
        int? labId, string? issuedTo, string? issuedBy,
        DateOnly? fromDate, DateOnly? toDate,
        List<int>? allowedLabIds, int page, int pageSize);
    Task ExtendStudentDueDateAsync(int issuanceId, ExtendDueDateDto dto, int userId);
    Task ExtendEmployeeDueDateAsync(int issuanceId, ExtendDueDateDto dto, int userId);
}