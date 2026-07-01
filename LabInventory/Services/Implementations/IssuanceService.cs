using LabInventory.Data;
using LabInventory.Models.DTOs.Issuance;
using LabInventory.Models.Entities;
using LabInventory.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Services.Implementations;

public class IssuanceService : IIssuanceService
{
    private readonly AppDbContext _db;
    private readonly IFineCalculationService _fineCalc;
    private readonly IAuditService _audit;

    public IssuanceService(AppDbContext db, IFineCalculationService fineCalc, IAuditService audit)
    {
        _db = db;
        _fineCalc = fineCalc;
        _audit = audit;
    }

    public async Task<StudentIssuance> IssueToStudentAsync(
        CreateStudentIssuanceDto dto, int issuedByUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var item = await _db.InventoryItems.FindAsync(dto.ItemId)
                ?? throw new InvalidOperationException("Inventory item not found.");

            if (item.RemainingQuantity < dto.QuantityIssued)
                throw new InvalidOperationException(
                    $"Only {item.RemainingQuantity} units available.");

            item.IssuedQuantity += dto.QuantityIssued;
            item.UpdatedAt = DateTime.UtcNow;

            var issuance = new StudentIssuance
            {
                LabId = dto.LabId,
                ItemId = dto.ItemId,
                QuantityIssued = dto.QuantityIssued,
                Student1Name = dto.Student1Name,
                RegistrationNo1 = dto.RegistrationNo1,
                ContactNo1 = dto.ContactNo1,
                Student2Name = dto.Student2Name,
                RegistrationNo2 = dto.RegistrationNo2,
                ContactNo2 = dto.ContactNo2,
                Student3Name = dto.Student3Name,
                RegistrationNo3 = dto.RegistrationNo3,
                DepartmentProgram = dto.DepartmentProgram,
                IssueDate = dto.IssueDate,
                DueDate = dto.DueDate,
                IssuedBy = issuedByUserId,
                Status = "Issued",
                FineAmount = 0,
                CreatedAt = DateTime.UtcNow,
                Remarks = dto.Remarks,
                ProjectName = dto.ProjectName,
                TeacherName = dto.TeacherName
            };

            _db.StudentIssuances.Add(issuance);

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ItemId = dto.ItemId,
                TransactionType = "ISSUE",
                Quantity = dto.QuantityIssued,
                ReferenceType = "STUDENT_ISSUANCE",
                PerformedBy = issuedByUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            await _audit.LogAsync(issuedByUserId, "Issued", "StudentIssuances",
                issuance.StudentIssuanceId.ToString(),
                newValues: $"{{Item: {dto.ItemId}, Qty: {dto.QuantityIssued}, Student: {dto.Student1Name}}}");

            await transaction.CommitAsync();
            return issuance;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<EmployeeIssuance> IssueToEmployeeAsync(
        CreateEmployeeIssuanceDto dto, int issuedByUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var item = await _db.InventoryItems.FindAsync(dto.ItemId)
                ?? throw new InvalidOperationException("Inventory item not found.");

            if (item.RemainingQuantity < dto.QuantityIssued)
                throw new InvalidOperationException(
                    $"Only {item.RemainingQuantity} units available.");

            item.IssuedQuantity += dto.QuantityIssued;
            item.UpdatedAt = DateTime.UtcNow;

            var issuance = new EmployeeIssuance
            {
                LabId = dto.LabId,
                ItemId = dto.ItemId,
                FacultyName = dto.FacultyName,
                Email = dto.Email,
                ContactNo = dto.ContactNo,
                Department = dto.Department,
                QuantityIssued = dto.QuantityIssued,
                IssueDate = dto.IssueDate,
                DueDate = dto.DueDate,
                IssuedBy = issuedByUserId,
                Status = "Issued",
                FineAmount = 0,
                CreatedAt = DateTime.UtcNow,
                Remarks = dto.Remarks
            };

            _db.EmployeeIssuances.Add(issuance);

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ItemId = dto.ItemId,
                TransactionType = "ISSUE",
                Quantity = dto.QuantityIssued,
                ReferenceType = "EMPLOYEE_ISSUANCE",
                PerformedBy = issuedByUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            await _audit.LogAsync(issuedByUserId, "Issued", "EmployeeIssuances",
                issuance.EmployeeIssuanceId.ToString(),
                newValues: $"{{Item: {dto.ItemId}, Qty: {dto.QuantityIssued}, Faculty: {dto.FacultyName}}}");

            await transaction.CommitAsync();
            return issuance;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ReturnStudentIssuanceAsync(
        int issuanceId, ReturnItemDto dto, int checkedByUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var issuance = await _db.StudentIssuances
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.StudentIssuanceId == issuanceId)
                ?? throw new InvalidOperationException("Issuance not found.");

            if (issuance.ReturnDate != null)
                throw new InvalidOperationException("Already returned.");

            var item = issuance.Item;
            var transactionType = "RETURN_GOOD";

            item.IssuedQuantity -= issuance.QuantityIssued;

            if (dto.Condition == "Broken")
            {
                item.DefectiveQuantity += issuance.QuantityIssued;
                transactionType = "RETURN_DEFECTIVE";
            }
            else if (dto.Condition == "Lost")
            {
                item.LostQuantity += issuance.QuantityIssued;
                transactionType = "RETURN_LOST";
            }

            decimal fine = _fineCalc.Calculate(
                issuance.ExtendedDueDate ?? issuance.DueDate, dto.ReturnDate,
                item.FinePerDay, issuance.QuantityIssued);

            issuance.ReturnDate = dto.ReturnDate;
            issuance.ConditionOnReturn = dto.Condition;
            issuance.FineAmount = fine;
            issuance.ReturnCheckedBy = checkedByUserId;
            issuance.Status = "Returned";

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ItemId = item.ItemId,
                TransactionType = transactionType,
                Quantity = issuance.QuantityIssued,
                ReferenceType = "STUDENT_ISSUANCE",
                ReferenceId = issuanceId,
                PerformedBy = checkedByUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            await _audit.LogAsync(checkedByUserId, "Returned", "StudentIssuances",
                issuanceId.ToString(),
                newValues: $"{{Condition: {dto.Condition}, Fine: {fine}, ReturnDate: {dto.ReturnDate}}}");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ReturnEmployeeIssuanceAsync(
        int issuanceId, ReturnItemDto dto, int checkedByUserId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var issuance = await _db.EmployeeIssuances
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.EmployeeIssuanceId == issuanceId)
                ?? throw new InvalidOperationException("Issuance not found.");

            if (issuance.ReturnDate != null)
                throw new InvalidOperationException("Already returned.");

            var item = issuance.Item;
            var transactionType = "RETURN_GOOD";

            item.IssuedQuantity -= issuance.QuantityIssued;

            if (dto.Condition == "Broken")
            {
                item.DefectiveQuantity += issuance.QuantityIssued;
                transactionType = "RETURN_DEFECTIVE";
            }
            else if (dto.Condition == "Lost")
            {
                item.LostQuantity += issuance.QuantityIssued;
                transactionType = "RETURN_LOST";
            }

            decimal fine = _fineCalc.Calculate(
                issuance.ExtendedDueDate ?? issuance.DueDate, dto.ReturnDate,
                item.FinePerDay, issuance.QuantityIssued);

            issuance.ReturnDate = dto.ReturnDate;
            issuance.ConditionOnReturn = dto.Condition;
            issuance.FineAmount = fine;
            issuance.ReturnCheckedBy = checkedByUserId;
            issuance.Status = "Returned";

            _db.InventoryTransactions.Add(new InventoryTransaction
            {
                ItemId = item.ItemId,
                TransactionType = transactionType,
                Quantity = issuance.QuantityIssued,
                ReferenceType = "EMPLOYEE_ISSUANCE",
                ReferenceId = issuanceId,
                PerformedBy = checkedByUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            await _audit.LogAsync(checkedByUserId, "Returned", "EmployeeIssuances",
                issuanceId.ToString(),
                newValues: $"{{Condition: {dto.Condition}, Fine: {fine}, ReturnDate: {dto.ReturnDate}}}");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ── allowedLabIds = null → admin, no restriction
    // ── allowedLabIds = [1,2] → only those labs
    public async Task<List<object>> GetActiveIssuancesAsync(int? labId, List<int>? allowedLabIds = null)
    {
        // FIX 1: studentQuery defined BEFORE use; IssuedByUser included; IssuedByName projected
        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Where(x => x.ReturnDate == null);

        if (labId.HasValue)
            studentQuery = studentQuery.Where(x => x.LabId == labId.Value);
        else if (allowedLabIds != null)
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var studentIssuances = await studentQuery
            .Include(x => x.IssuedByUser)
            .Select(x => new
            {
                Id = x.StudentIssuanceId,
                Type = "Student",
                Name = x.Student1Name,
                Item = x.Item.EquipmentName,
                x.LabId,
                x.QuantityIssued,
                x.IssueDate,

                x.DueDate,

                ExtendedDueDate = x.ExtendedDueDate,

                EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate,

                x.Status,
                IssuedByName = x.IssuedByUser.FullName
            }).ToListAsync();

        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Where(x => x.ReturnDate == null);

        if (labId.HasValue)
            employeeQuery = employeeQuery.Where(x => x.LabId == labId.Value);
        else if (allowedLabIds != null)
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var employeeIssuances = await employeeQuery
            .Include(x => x.IssuedByUser)
            .Select(x => new
            {
                Id = x.EmployeeIssuanceId,
                Type = "Employee",
                Name = x.FacultyName,
                Item = x.Item.EquipmentName,
                x.LabId,
                x.QuantityIssued,
                x.IssueDate,
                x.DueDate,
                x.Status,
                IssuedByName = x.IssuedByUser.FullName
            }).ToListAsync();

        return studentIssuances
            .Cast<object>()
            .Concat(employeeIssuances.Cast<object>())
            .ToList();
    }

    public async Task<List<object>> GetOverdueIssuancesAsync(List<int>? allowedLabIds = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // FIX 2: IssuedByUser included; IssuedByName projected
        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Include(x => x.IssuedByUser)
            .Where(x => x.ReturnDate == null && x.DueDate < today);

        if (allowedLabIds != null)
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var students = await studentQuery
            .Select(x => new
            {
                Type = "Student",
                Name = x.Student1Name,
                Item = x.Item.EquipmentName,
                IssuedByName = x.IssuedByUser.FullName,
                x.DueDate,
                DaysOverdue = today.DayNumber - x.DueDate!.Value.DayNumber
            }).ToListAsync();

        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Include(x => x.IssuedByUser)
            .Where(x => x.ReturnDate == null && x.DueDate < today);

        if (allowedLabIds != null)
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));

        var employees = await employeeQuery
            .Select(x => new
            {
                Type = "Employee",
                Name = x.FacultyName,
                Item = x.Item.EquipmentName,
                IssuedByName = x.IssuedByUser.FullName,
                x.DueDate,
                DaysOverdue = today.DayNumber - x.DueDate!.Value.DayNumber
            }).ToListAsync();

        return students
            .Cast<object>()
            .Concat(employees.Cast<object>())
            .ToList();
    }

    public async Task<object?> GetStudentIssuanceByIdAsync(int id)
    {
        return await _db.StudentIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.IssuedByUser)
            .Where(x => x.StudentIssuanceId == id)
            .Select(x => (object)new
            {
                x.StudentIssuanceId,
                x.LabId,
                LabName = x.Lab.LabName,
                x.ItemId,
                ItemName = x.Item.EquipmentName,
                FinePerDay = x.Item.FinePerDay,
                x.QuantityIssued,
                x.Student1Name,
                x.RegistrationNo1,
                x.ContactNo1,
                x.Student2Name,
                x.RegistrationNo2,
                x.ContactNo2,
                x.Student3Name,
                x.RegistrationNo3,
                x.DepartmentProgram,
                x.IssueDate,
                x.DueDate,
                x.ExtendedDueDate,           // ← ADD THIS
                EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate,
                x.ReturnDate,
                x.ConditionOnReturn,
                x.FineAmount,
                x.Status,
                x.Remarks,
                IssuedByName = x.IssuedByUser.FullName,
                x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<object?> GetEmployeeIssuanceByIdAsync(int id)
    {
        return await _db.EmployeeIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.IssuedByUser)
            .Where(x => x.EmployeeIssuanceId == id)
            .Select(x => (object)new
            {
                x.EmployeeIssuanceId,
                x.LabId,
                LabName = x.Lab.LabName,
                x.ItemId,
                ItemName = x.Item.EquipmentName,
                FinePerDay = x.Item.FinePerDay,
                x.QuantityIssued,
                x.FacultyName,
                x.Email,
                x.ContactNo,
                x.Department,
                x.IssueDate,
                x.DueDate,
                x.ExtendedDueDate,           // ← ADD THIS
                EffectiveDueDate = x.ExtendedDueDate ?? x.DueDate,
                x.ReturnDate,
                x.ConditionOnReturn,
                x.FineAmount,
                x.Status,
                x.Remarks,
                IssuedByName = x.IssuedByUser.FullName,
                x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> BulkIssueToStudentAsync(BulkStudentIssuanceDto dto, int issuedByUserId)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new InvalidOperationException("No items provided.");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var line in dto.Items)
            {
                var item = await _db.InventoryItems.FindAsync(line.ItemId)
                    ?? throw new InvalidOperationException($"Item ID {line.ItemId} not found.");

                if (item.RemainingQuantity < line.QuantityIssued)
                    throw new InvalidOperationException(
                        $"'{item.EquipmentName}' only has {item.RemainingQuantity} units available " +
                        $"but {line.QuantityIssued} requested.");

                item.IssuedQuantity += line.QuantityIssued;
                item.UpdatedAt = DateTime.UtcNow;

                var issuance = new StudentIssuance
                {
                    LabId = line.LabId,
                    ItemId = line.ItemId,
                    QuantityIssued = line.QuantityIssued,
                    Student1Name = dto.Student1Name,
                    RegistrationNo1 = dto.RegistrationNo1,
                    ContactNo1 = dto.ContactNo1,
                    Student2Name = dto.Student2Name,
                    RegistrationNo2 = dto.RegistrationNo2,
                    ContactNo2 = dto.ContactNo2,
                    Student3Name = dto.Student3Name,
                    RegistrationNo3 = dto.RegistrationNo3,
                    DepartmentProgram = dto.DepartmentProgram,
                    IssueDate = dto.IssueDate,
                    DueDate = dto.DueDate,
                    IssuedBy = issuedByUserId,
                    Status = "Issued",
                    FineAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Remarks = dto.Remarks,
                    ProjectName = dto.ProjectName,
                    TeacherName = dto.TeacherName
                };

                _db.StudentIssuances.Add(issuance);

                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    ItemId = line.ItemId,
                    TransactionType = "ISSUE",
                    Quantity = line.QuantityIssued,
                    ReferenceType = "STUDENT_ISSUANCE",
                    PerformedBy = issuedByUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            await _audit.LogAsync(issuedByUserId, "BulkIssued", "StudentIssuances",
                "bulk",
                newValues: $"{{Student: {dto.Student1Name}, Items: {dto.Items.Count}, " +
                           $"TotalQty: {dto.Items.Sum(i => i.QuantityIssued)}}}");

            await transaction.CommitAsync();
            return dto.Items.Count;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // FIX 3: New history method with filtering and pagination
    public async Task<object> GetIssuanceHistoryAsync(
        int? labId, string? issuedTo, string? issuedBy,
        DateOnly? fromDate, DateOnly? toDate,
        List<int>? allowedLabIds, int page, int pageSize)
    {
        var studentQuery = _db.StudentIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.IssuedByUser)
            .AsQueryable();

        var employeeQuery = _db.EmployeeIssuances
            .Include(x => x.Item)
            .Include(x => x.Lab)
            .Include(x => x.IssuedByUser)
            .AsQueryable();

        if (labId.HasValue)
        {
            studentQuery = studentQuery.Where(x => x.LabId == labId.Value);
            employeeQuery = employeeQuery.Where(x => x.LabId == labId.Value);
        }
        else if (allowedLabIds != null)
        {
            studentQuery = studentQuery.Where(x => allowedLabIds.Contains(x.LabId));
            employeeQuery = employeeQuery.Where(x => allowedLabIds.Contains(x.LabId));
        }

        if (fromDate.HasValue)
        {
            studentQuery = studentQuery.Where(x => x.IssueDate >= fromDate.Value);
            employeeQuery = employeeQuery.Where(x => x.IssueDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            studentQuery = studentQuery.Where(x => x.IssueDate <= toDate.Value);
            employeeQuery = employeeQuery.Where(x => x.IssueDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(issuedTo))
        {
            studentQuery = studentQuery.Where(x => x.Student1Name.Contains(issuedTo));
            employeeQuery = employeeQuery.Where(x => x.FacultyName.Contains(issuedTo));
        }

        if (!string.IsNullOrWhiteSpace(issuedBy))
        {
            studentQuery = studentQuery.Where(x => x.IssuedByUser.FullName.Contains(issuedBy));
            employeeQuery = employeeQuery.Where(x => x.IssuedByUser.FullName.Contains(issuedBy));
        }

        var students = await studentQuery.Select(x => new
        {
            Id = x.StudentIssuanceId,
            Type = "Student",
            IssuedTo = x.Student1Name,
            RegistrationNo = x.RegistrationNo1,
            IssuedByName = x.IssuedByUser.FullName,
            Lab = x.Lab.LabName,
            x.LabId,
            Item = x.Item.EquipmentName,
            x.QuantityIssued,
            x.IssueDate,
            x.DueDate,
            x.ReturnDate,
            x.FineAmount,
            x.Status
        }).ToListAsync();

        var employees = await employeeQuery.Select(x => new
        {
            Id = x.EmployeeIssuanceId,
            Type = "Employee",
            IssuedTo = x.FacultyName,
            RegistrationNo = (string?)null,
            IssuedByName = x.IssuedByUser.FullName,
            Lab = x.Lab.LabName,
            x.LabId,
            Item = x.Item.EquipmentName,
            x.QuantityIssued,
            x.IssueDate,
            x.DueDate,
            x.ReturnDate,
            x.FineAmount,
            x.Status
        }).ToListAsync();

        var combined = students
            .Cast<dynamic>()
            .Concat(employees.Cast<dynamic>())
            .OrderByDescending(x => (DateOnly)x.IssueDate)
            .ToList();

        var total = combined.Count;
        var paged = combined.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new { total, page, pageSize, records = paged };
    }
    public async Task ExtendStudentDueDateAsync(int issuanceId, ExtendDueDateDto dto, int userId)
    {
        var issuance = await _db.StudentIssuances.FindAsync(issuanceId)
            ?? throw new InvalidOperationException("Issuance not found.");

        if (issuance.ReturnDate != null)
            throw new InvalidOperationException("Cannot extend due date of a returned issuance.");

        if (dto.NewDueDate <= (issuance.ExtendedDueDate ?? issuance.DueDate))
            throw new InvalidOperationException("New due date must be later than the current due date.");

        issuance.ExtendedDueDate = dto.NewDueDate;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "DueDateExtended", "StudentIssuances",
            issuanceId.ToString(),
            newValues: $"{{NewDueDate: {dto.NewDueDate}, Remarks: {dto.Remarks}}}");
    }

    public async Task ExtendEmployeeDueDateAsync(int issuanceId, ExtendDueDateDto dto, int userId)
    {
        var issuance = await _db.EmployeeIssuances.FindAsync(issuanceId)
            ?? throw new InvalidOperationException("Issuance not found.");

        if (issuance.ReturnDate != null)
            throw new InvalidOperationException("Cannot extend due date of a returned issuance.");

        if (dto.NewDueDate <= (issuance.ExtendedDueDate ?? issuance.DueDate))
            throw new InvalidOperationException("New due date must be later than the current due date.");

        issuance.ExtendedDueDate = dto.NewDueDate;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(userId, "DueDateExtended", "EmployeeIssuances",
            issuanceId.ToString(),
            newValues: $"{{NewDueDate: {dto.NewDueDate}, Remarks: {dto.Remarks}}}");
    }
}