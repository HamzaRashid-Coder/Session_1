using LabInventory.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserLab> UserLabs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<StudentIssuance> StudentIssuances { get; set; }
        public DbSet<EmployeeIssuance> EmployeeIssuances { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Table names & primary keys ──────────────────────────
            modelBuilder.Entity<User>().ToTable("Users").HasKey(u => u.UserId);
            modelBuilder.Entity<Role>().ToTable("Roles").HasKey(r => r.RoleId);
            modelBuilder.Entity<Permission>().ToTable("Permissions").HasKey(p => p.PermissionId);
            modelBuilder.Entity<Lab>().ToTable("Labs").HasKey(l => l.LabId);
            modelBuilder.Entity<UserRole>().ToTable("UserRoles").HasKey(ur => ur.UserRoleId);
            modelBuilder.Entity<UserLab>().ToTable("UserLabs").HasKey(ul => ul.UserLabId);
            modelBuilder.Entity<RolePermission>().ToTable("RolePermissions").HasKey(rp => rp.RolePermissionId);
            modelBuilder.Entity<InventoryItem>().ToTable("InventoryItems").HasKey(i => i.ItemId);
            modelBuilder.Entity<StudentIssuance>().ToTable("StudentIssuances").HasKey(s => s.StudentIssuanceId);
            modelBuilder.Entity<EmployeeIssuance>().ToTable("EmployeeIssuances").HasKey(e => e.EmployeeIssuanceId);
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs").HasKey(a => a.AuditLogId);
            modelBuilder.Entity<InventoryTransaction>().ToTable("InventoryTransactions").HasKey(t => t.TransactionId);

            // ── Computed column — never written by EF ───────────────
            modelBuilder.Entity<InventoryItem>()
                .Property(i => i.RemainingQuantity)
                .ValueGeneratedOnAddOrUpdate()
                .HasComputedColumnSql(
                    "TotalQuantity - IssuedQuantity - DefectiveQuantity - LostQuantity"
                );

            // ── Unique indexes ───────────────────────────────────────
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            modelBuilder.Entity<UserLab>()
                .HasIndex(ul => new { ul.UserId, ul.LabId }).IsUnique();
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

            // ── Lab → CreatedByUser ──────────────────────────────────
            modelBuilder.Entity<Lab>()
                .HasOne(l => l.CreatedByUser)
                .WithMany()
                .HasForeignKey(l => l.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── InventoryItem → Lab ──────────────────────────────────
            modelBuilder.Entity<InventoryItem>()
                .HasOne(i => i.Lab)
                .WithMany(l => l.InventoryItems)
                .HasForeignKey(i => i.LabId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── InventoryItem → CreatedByUser ────────────────────────
            modelBuilder.Entity<InventoryItem>()
                .HasOne(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── UserRole relationships ───────────────────────────────
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── UserLab relationships ────────────────────────────────
            modelBuilder.Entity<UserLab>()
                .HasOne(ul => ul.User)
                .WithMany()
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLab>()
                .HasOne(ul => ul.Lab)
                .WithMany(l => l.UserLabs)
                .HasForeignKey(ul => ul.LabId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── RolePermission relationships ─────────────────────────
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── StudentIssuance — three FKs to Users ─────────────────
            modelBuilder.Entity<StudentIssuance>()
                .HasOne(s => s.IssuedByUser)
                .WithMany()
                .HasForeignKey(s => s.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentIssuance>()
                .HasOne(s => s.ReturnCheckedByUser)
                .WithMany()
                .HasForeignKey(s => s.ReturnCheckedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentIssuance>()
                .HasOne(s => s.FinePaidByUser)        // ← new
                .WithMany()
                .HasForeignKey(s => s.FinePaidBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentIssuance>()
                .HasOne(s => s.Lab)
                .WithMany()
                .HasForeignKey(s => s.LabId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentIssuance>()
                .HasOne(s => s.Item)
                .WithMany(i => i.StudentIssuances)
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── EmployeeIssuance — three FKs to Users ────────────────
            modelBuilder.Entity<EmployeeIssuance>()
                .HasOne(e => e.IssuedByUser)
                .WithMany()
                .HasForeignKey(e => e.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeIssuance>()
                .HasOne(e => e.ReturnCheckedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReturnCheckedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeIssuance>()
                .HasOne(e => e.FinePaidByUser)        // ← new
                .WithMany()
                .HasForeignKey(e => e.FinePaidBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeIssuance>()
                .HasOne(e => e.Lab)
                .WithMany()
                .HasForeignKey(e => e.LabId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeIssuance>()
                .HasOne(e => e.Item)
                .WithMany(i => i.EmployeeIssuances)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── InventoryTransaction ─────────────────────────────────
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.Item)
                .WithMany()
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.PerformedByUser)
                .WithMany()
                .HasForeignKey(t => t.PerformedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── AuditLog ─────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}