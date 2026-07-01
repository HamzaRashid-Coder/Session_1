using Microsoft.AspNetCore.Authorization;

namespace LabInventory.Authentication.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User
            .FindAll("permissions")
            .Select(c => c.Value)
            .ToList();

        if (permissions.Contains(requirement.PermissionKey))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}