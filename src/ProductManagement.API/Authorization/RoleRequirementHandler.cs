using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProductManagement.API.Authorization;

/// <summary>
/// Handler for role-based authorization
/// </summary>
public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        // Check if user has any of the required roles
        var hasRole = requirement.Roles.Any(role =>
            context.User.HasClaim(c =>
                c.Type == ClaimTypes.Role &&
                c.Value.Equals(role, StringComparison.OrdinalIgnoreCase)));

        if (hasRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}