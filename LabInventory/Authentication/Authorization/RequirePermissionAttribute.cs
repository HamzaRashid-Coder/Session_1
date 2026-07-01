using Microsoft.AspNetCore.Authorization;

namespace LabInventory.Authentication.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionKey)
        : base(policy: permissionKey)
    {
    }
}