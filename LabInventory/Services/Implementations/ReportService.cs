using ClosedXML.Excel;
using LabInventory.Data;
using LabInventory.Models.DTOs.Reports;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<object>> GetStudentIssuancesAsync(ReportFilterDto filter)
    {
        var query = _db.StudentIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .AsQueryable();

        if (filter.LabId.HasValue)
            query = query.Where(x => x.LabId == filter.LabId.Value);
        else if (filter.AllowedLabIds != null)
            query = query.Where(x => filter.AllowedLabIds.Contains(x.LabId));

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.IssueDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.IssueDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchName))
            query = query.Where(x => x.Student1Name.Contains(filter.SearchName));

        if (!string.IsNullOrWhiteSpace(filter.RegistrationNo))
            query = query.Where(x => x.RegistrationNo1.Contains(filter.RegistrationNo));

        return await query.OrderByDescending(x => x.IssueDate)
         .Select(x => (object)new
         {
             x.StudentIssuanceId,
             x.Student1Name,
             x.RegistrationNo1,
             x.DepartmentProgram,
             Item = x.Item.EquipmentName,
             Lab = x.Lab.LabName,
             x.QuantityIssued,
             x.IssueDate,

             DueDate = x.DueDate,
             ExtendedDueDate = x.ExtendedDueDate,

             EffectiveDueDate =
                 x.ExtendedDueDate ?? x.DueDate,

             x.ReturnDate,
             x.ConditionOnReturn,
             x.FineAmount,
             x.Status
         })
         .ToListAsync();
    }

    public async Task<List<object>> GetEmployeeIssuancesAsync(ReportFilterDto filter)
    {
        var query = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .AsQueryable();

        if (filter.LabId.HasValue)
            query = query.Where(x => x.LabId == filter.LabId.Value);
        else if (filter.AllowedLabIds != null)
            query = query.Where(x => filter.AllowedLabIds.Contains(x.LabId));

        if (filter.FromDate.HasValue)
            query = query.Where(x => x.IssueDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.IssueDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchName))
            query = query.Where(x => x.FacultyName.Contains(filter.SearchName));

        return await query.OrderByDescending(x => x.IssueDate)
           .Select(x => (object)new
           {
               x.EmployeeIssuanceId,
               x.FacultyName,
               x.Department,
               x.Email,
               x.ContactNo,
               Item = x.Item.EquipmentName,
               Lab = x.Lab.LabName,
               x.QuantityIssued,
               x.IssueDate,

               DueDate = x.DueDate,
               ExtendedDueDate = x.ExtendedDueDate,
               EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate,

               x.ReturnDate,
               x.ConditionOnReturn,
               x.FineAmount,
               x.Status
           }).ToListAsync();
    }

    public async Task<List<object>> GetOverdueAsync(List<int>? allowedLabIds)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Where(x => x.ReturnDate == null && (x.ExtendedDueDate ?? x.DueDate) < today && (x.ExtendedDueDate ?? x.DueDate) != null)
            .AsQueryable();

        if (allowedLabIds != null)
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var students = await studentQuery
            .Select(x => (object)new
            {
                Type = "Student",
                Name = x.Student1Name,
                Item = x.Item.EquipmentName,
                x.IssueDate,
                x.DueDate,
                EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate,

                DaysOverdue = today.DayNumber - (x.ExtendedDueDate ?? x.DueDate)!.Value.DayNumber
            })
            .ToListAsync();

        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Where(x => x.ReturnDate == null && (x.ExtendedDueDate ?? x.DueDate) < today && (x.ExtendedDueDate ?? x.DueDate) != null)
            .AsQueryable();

        if (allowedLabIds != null)
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var employees = await employeeQuery
            .Select(x => (object)new
            {
                Type = "Employee",
                Name = x.FacultyName,
                Item = x.Item.EquipmentName,
                x.IssueDate,
                x.DueDate,
                EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate, 

                DaysOverdue = today.DayNumber - (x.ExtendedDueDate ?? x.DueDate)!.Value.DayNumber
            })
            .ToListAsync();

        return students.Concat(employees).ToList();
    }

    public async Task<List<object>> GetFinesAsync(List<int>? allowedLabIds)
    {
        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Where(x => x.FineAmount > 0)
            .AsQueryable();

        if (allowedLabIds != null)
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var studentFines = await studentQuery
            .Select(x => (object)new
            {
                Type = "Student",
                Name = x.Student1Name,
                Item = x.Item.EquipmentName,
                x.IssueDate,
                x.ReturnDate,
                x.DueDate,
                x.FineAmount
            })
            .ToListAsync();

        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Where(x => x.FineAmount > 0)
            .AsQueryable();

        if (allowedLabIds != null)
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var employeeFines = await employeeQuery
            .Select(x => (object)new
            {
                Type = "Employee",
                Name = x.FacultyName,
                Item = x.Item.EquipmentName,
                x.IssueDate,
                x.ReturnDate,
                x.DueDate,
                x.FineAmount
            })
            .ToListAsync();

        return studentFines.Concat(employeeFines).ToList();
    }

    public async Task<List<object>> GetDefectiveLostAsync(List<int>? allowedLabIds)
    {
        var query = _db.InventoryItems
            .Include(x => x.Lab)
            .Where(x => x.DefectiveQuantity > 0 || x.LostQuantity > 0)
            .AsQueryable();

        if (allowedLabIds != null)
            query = query.Where(x => allowedLabIds.Contains(x.LabId));

        return await query
            .Select(x => (object)new
            {
                x.ItemId,
                x.EquipmentName,
                x.ModelNumber,
                Lab = x.Lab.LabName,
                x.TotalQuantity,
                x.DefectiveQuantity,
                x.LostQuantity,
                x.RemainingQuantity
            })
            .ToListAsync();
    }

    public async Task<List<object>> GetLabInventoryAsync(int labId)
    {
        return await _db.InventoryItems
            .Include(x => x.Lab)
            .Where(x => x.LabId == labId)
            .Select(x => (object)new
            {
                x.ItemId,
                x.EquipmentName,
                x.ModelNumber,
                x.Placement,
                x.FinePerDay,
                x.TotalQuantity,
                x.IssuedQuantity,
                x.RemainingQuantity,
                x.DefectiveQuantity,
                x.LostQuantity
            })
            .ToListAsync();
    }

    public async Task<byte[]> GenerateExcelAsync(string reportType, List<int>? allowedLabIds)
    {
        using var workbook = new XLWorkbook();

        if (reportType == "student-issuances")
        {
            var query = _db.StudentIssuances.Include(x => x.Item).Include(x => x.Lab).AsQueryable();
            if (allowedLabIds != null)
                query = query.Where(x => allowedLabIds.Contains(x.LabId));
            var data = await query.ToListAsync();

            var sheet = workbook.Worksheets.Add("Student Issuances");
            sheet.Cell(1, 1).Value = "Student";
            sheet.Cell(1, 2).Value = "Reg No";
            sheet.Cell(1, 3).Value = "Department";
            sheet.Cell(1, 4).Value = "Item";
            sheet.Cell(1, 5).Value = "Lab";
            sheet.Cell(1, 6).Value = "Qty";
            sheet.Cell(1, 7).Value = "Issue Date";
            sheet.Cell(1, 8).Value = "Due Date";
            sheet.Cell(1, 9).Value = "Return Date";
            sheet.Cell(1, 10).Value = "Condition";
            sheet.Cell(1, 11).Value = "Fine";
            sheet.Cell(1, 12).Value = "Status";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var r = data[i];
                sheet.Cell(row, 1).Value = r.Student1Name;
                sheet.Cell(row, 2).Value = r.RegistrationNo1;
                sheet.Cell(row, 3).Value = r.DepartmentProgram ?? "";
                sheet.Cell(row, 4).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 5).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 6).Value = r.QuantityIssued;
                sheet.Cell(row, 7).Value = r.IssueDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 8).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "No Due Date";
                sheet.Cell(row, 9).Value = r.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned";
                sheet.Cell(row, 10).Value = r.ConditionOnReturn ?? "";
                sheet.Cell(row, 11).Value = r.FineAmount;
                sheet.Cell(row, 12).Value = r.Status;
            }
            sheet.Columns().AdjustToContents();
        }
        else if (reportType == "employee-issuances")
        {
            var query = _db.EmployeeIssuances.Include(x => x.Item).Include(x => x.Lab).AsQueryable();
            if (allowedLabIds != null)
                query = query.Where(x => allowedLabIds.Contains(x.LabId));
            var data = await query.ToListAsync();

            var sheet = workbook.Worksheets.Add("Employee Issuances");
            sheet.Cell(1, 1).Value = "Faculty";
            sheet.Cell(1, 2).Value = "Department";
            sheet.Cell(1, 3).Value = "Email";
            sheet.Cell(1, 4).Value = "Item";
            sheet.Cell(1, 5).Value = "Lab";
            sheet.Cell(1, 6).Value = "Qty";
            sheet.Cell(1, 7).Value = "Issue Date";
            sheet.Cell(1, 8).Value = "Due Date";
            sheet.Cell(1, 9).Value = "Return Date";
            sheet.Cell(1, 10).Value = "Condition";
            sheet.Cell(1, 11).Value = "Fine";
            sheet.Cell(1, 12).Value = "Status";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var r = data[i];
                sheet.Cell(row, 1).Value = r.FacultyName;
                sheet.Cell(row, 2).Value = r.Department ?? "";
                sheet.Cell(row, 3).Value = r.Email ?? "";
                sheet.Cell(row, 4).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 5).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 6).Value = r.QuantityIssued;
                sheet.Cell(row, 7).Value = r.IssueDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 8).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "No Due Date";
                sheet.Cell(row, 9).Value = r.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned";
                sheet.Cell(row, 10).Value = r.ConditionOnReturn ?? "";
                sheet.Cell(row, 11).Value = r.FineAmount;
                sheet.Cell(row, 12).Value = r.Status;
            }
            sheet.Columns().AdjustToContents();
        }
        else if (reportType == "overdue")
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var sheet = workbook.Worksheets.Add("Overdue");
            sheet.Cell(1, 1).Value = "Type";
            sheet.Cell(1, 2).Value = "Name";
            sheet.Cell(1, 3).Value = "Item";
            sheet.Cell(1, 4).Value = "Lab";
            sheet.Cell(1, 5).Value = "Issue Date";
            sheet.Cell(1, 6).Value = "Due Date";
            sheet.Cell(1, 7).Value = "Days Overdue";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            var studentQuery = _db.StudentIssuances.Include(x => x.Item).Include(x => x.Lab)
                .Where(x => x.ReturnDate == null && x.DueDate < today && x.DueDate != null)
                .AsQueryable();
            if (allowedLabIds != null)
                studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));
            var students = await studentQuery.ToListAsync();

            var employeeQuery = _db.EmployeeIssuances.Include(x => x.Item).Include(x => x.Lab)
                .Where(x => x.ReturnDate == null && x.DueDate < today && x.DueDate != null)
                .AsQueryable();
            if (allowedLabIds != null)
                employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));
            var employees = await employeeQuery.ToListAsync();

            int row = 2;
            foreach (var r in students)
            {
                sheet.Cell(row, 1).Value = "Student";
                sheet.Cell(row, 2).Value = r.Student1Name;
                sheet.Cell(row, 3).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 4).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 5).Value = r.IssueDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 6).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 7).Value = today.DayNumber - r.DueDate!.Value.DayNumber;
                row++;
            }
            foreach (var r in employees)
            {
                sheet.Cell(row, 1).Value = "Employee";
                sheet.Cell(row, 2).Value = r.FacultyName;
                sheet.Cell(row, 3).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 4).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 5).Value = r.IssueDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 6).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 7).Value = today.DayNumber - r.DueDate!.Value.DayNumber;
                row++;
            }
            sheet.Columns().AdjustToContents();
        }
        else if (reportType == "defective-lost")
        {
            var query = _db.InventoryItems.Include(x => x.Lab)
                .Where(x => x.DefectiveQuantity > 0 || x.LostQuantity > 0)
                .AsQueryable();
            if (allowedLabIds != null)
                query = query.Where(x => allowedLabIds.Contains(x.LabId));
            var data = await query.ToListAsync();

            var sheet = workbook.Worksheets.Add("Defective & Lost");
            sheet.Cell(1, 1).Value = "Equipment";
            sheet.Cell(1, 2).Value = "Model";
            sheet.Cell(1, 3).Value = "Lab";
            sheet.Cell(1, 4).Value = "Total Qty";
            sheet.Cell(1, 5).Value = "Defective";
            sheet.Cell(1, 6).Value = "Lost";
            sheet.Cell(1, 7).Value = "Remaining";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var r = data[i];
                sheet.Cell(row, 1).Value = r.EquipmentName;
                sheet.Cell(row, 2).Value = r.ModelNumber ?? "";
                sheet.Cell(row, 3).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 4).Value = r.TotalQuantity;
                sheet.Cell(row, 5).Value = r.DefectiveQuantity;
                sheet.Cell(row, 6).Value = r.LostQuantity;
                sheet.Cell(row, 7).Value = r.RemainingQuantity;
            }
            sheet.Columns().AdjustToContents();
        }
        else if (reportType == "fines")
        {
            var sheet = workbook.Worksheets.Add("Fines");
            sheet.Cell(1, 1).Value = "Type";
            sheet.Cell(1, 2).Value = "Name";
            sheet.Cell(1, 3).Value = "Item";
            sheet.Cell(1, 4).Value = "Due Date";
            sheet.Cell(1, 5).Value = "Return Date";
            sheet.Cell(1, 6).Value = "Fine Amount";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            var studentQuery = _db.StudentIssuances.Include(x => x.Item)
                .Where(x => x.FineAmount > 0).AsQueryable();
            if (allowedLabIds != null)
                studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));
            var studentFines = await studentQuery.ToListAsync();

            var employeeQuery = _db.EmployeeIssuances.Include(x => x.Item)
                .Where(x => x.FineAmount > 0).AsQueryable();
            if (allowedLabIds != null)
                employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));
            var employeeFines = await employeeQuery.ToListAsync();

            int row = 2;
            foreach (var r in studentFines)
            {
                sheet.Cell(row, 1).Value = "Student";
                sheet.Cell(row, 2).Value = r.Student1Name;
                sheet.Cell(row, 3).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 4).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 5).Value = r.ReturnDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 6).Value = r.FineAmount;
                row++;
            }
            foreach (var r in employeeFines)
            {
                sheet.Cell(row, 1).Value = "Employee";
                sheet.Cell(row, 2).Value = r.FacultyName;
                sheet.Cell(row, 3).Value = r.Item?.EquipmentName ?? "";
                sheet.Cell(row, 4).Value = r.DueDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 5).Value = r.ReturnDate?.ToString("yyyy-MM-dd") ?? "";
                sheet.Cell(row, 6).Value = r.FineAmount;
                row++;
            }
            sheet.Columns().AdjustToContents();
        }
        else
        {
            var query = _db.InventoryItems.Include(x => x.Lab).AsQueryable();
            if (allowedLabIds != null)
                query = query.Where(x => allowedLabIds.Contains(x.LabId));
            var data = await query.ToListAsync();

            var sheet = workbook.Worksheets.Add("Inventory");
            sheet.Cell(1, 1).Value = "Lab";
            sheet.Cell(1, 2).Value = "Equipment";
            sheet.Cell(1, 3).Value = "Model";
            sheet.Cell(1, 4).Value = "Total";
            sheet.Cell(1, 5).Value = "Issued";
            sheet.Cell(1, 6).Value = "Remaining";
            sheet.Cell(1, 7).Value = "Defective";
            sheet.Cell(1, 8).Value = "Lost";
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                var r = data[i];
                sheet.Cell(row, 1).Value = r.Lab?.LabName ?? "";
                sheet.Cell(row, 2).Value = r.EquipmentName;
                sheet.Cell(row, 3).Value = r.ModelNumber ?? "";
                sheet.Cell(row, 4).Value = r.TotalQuantity;
                sheet.Cell(row, 5).Value = r.IssuedQuantity;
                sheet.Cell(row, 6).Value = r.RemainingQuantity;
                sheet.Cell(row, 7).Value = r.DefectiveQuantity;
                sheet.Cell(row, 8).Value = r.LostQuantity;
            }
            sheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}