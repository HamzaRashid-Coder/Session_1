using LabInventory.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext db)
        {
            // Only seed if no permissions exist yet
            if (await db.Permissions.AnyAsync()) return;

            // ── Permissions ─────────────────────────────────────────
            var permissions = new List<Permission>
            {
                new() { PermissionKey = "inventory.create",          PermissionName = "Create Inventory",          ModuleName = "Inventory" },
                new() { PermissionKey = "inventory.read",            PermissionName = "View Inventory",            ModuleName = "Inventory" },
                new() { PermissionKey = "inventory.update",          PermissionName = "Update Inventory",          ModuleName = "Inventory" },
                new() { PermissionKey = "inventory.delete",          PermissionName = "Delete Inventory",          ModuleName = "Inventory" },
                new() { PermissionKey = "student_issuance.create",   PermissionName = "Create Student Issuance",   ModuleName = "Student Issuance" },
                new() { PermissionKey = "student_issuance.read",     PermissionName = "View Student Issuance",     ModuleName = "Student Issuance" },
                new() { PermissionKey = "student_issuance.update",   PermissionName = "Update Student Issuance",   ModuleName = "Student Issuance" },
                new() { PermissionKey = "student_issuance.delete",   PermissionName = "Delete Student Issuance",   ModuleName = "Student Issuance" },
                new() { PermissionKey = "employee_issuance.create",  PermissionName = "Create Employee Issuance",  ModuleName = "Employee Issuance" },
                new() { PermissionKey = "employee_issuance.read",    PermissionName = "View Employee Issuance",    ModuleName = "Employee Issuance" },
                new() { PermissionKey = "employee_issuance.update",  PermissionName = "Update Employee Issuance",  ModuleName = "Employee Issuance" },
                new() { PermissionKey = "employee_issuance.delete",  PermissionName = "Delete Employee Issuance",  ModuleName = "Employee Issuance" },
                new() { PermissionKey = "reports.read",              PermissionName = "View Reports",              ModuleName = "Reports" },
                new() { PermissionKey = "dashboard.read",            PermissionName = "View Dashboard",            ModuleName = "Dashboard" },
                new() { PermissionKey = "labs.create",               PermissionName = "Create Labs",               ModuleName = "Labs" },
                new() { PermissionKey = "labs.read",                 PermissionName = "View Labs",                 ModuleName = "Labs" },
                new() { PermissionKey = "labs.update",               PermissionName = "Update Labs",               ModuleName = "Labs" },
                new() { PermissionKey = "labs.delete",               PermissionName = "Delete Labs",               ModuleName = "Labs" },
                new() { PermissionKey = "users.manage",              PermissionName = "Manage Users",              ModuleName = "Users" },
                new() { PermissionKey = "roles.manage",              PermissionName = "Manage Roles",              ModuleName = "Roles" },
                new() { PermissionKey = "fines.manage",              PermissionName = "Manage Fines",              ModuleName = "Fines" },  // ← added
            };

            db.Permissions.AddRange(permissions);
            await db.SaveChangesAsync();

            // ── Admin Role ───────────────────────────────────────────
            var adminRole = new Role
            {
                RoleName = "Admin",
                Description = "System Administrator",
                CreatedAt = DateTime.UtcNow
            };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();

            // Assign ALL permissions to Admin role
            var allPermissions = await db.Permissions.ToListAsync();
            db.RolePermissions.AddRange(allPermissions.Select(p => new RolePermission
            {
                RoleId = adminRole.RoleId,
                PermissionId = p.PermissionId
            }));
            await db.SaveChangesAsync();

            // ── Default Admin User ───────────────────────────────────
            var adminUser = new User
            {
                FullName = "System Administrator",
                Email = "admin@system.com",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PhoneNumber = "0000000000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();

            // Assign Admin role to admin user
            db.UserRoles.Add(new UserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId,
                AssignedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}