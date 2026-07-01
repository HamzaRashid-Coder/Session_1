namespace LabInventory.Models.Entities
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionKey { get; set; }    // e.g. "inventory.create"
        public string PermissionName { get; set; }
        public string ModuleName { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
