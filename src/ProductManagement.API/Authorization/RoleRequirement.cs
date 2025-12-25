using Microsoft.AspNetCore.Authorization;

namespace ProductManagement.API.Authorization;

/// <summary>
/// Custom authorization requirement for role-based access
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    public string[] Roles { get; }

    public RoleRequirement(params string[] roles)
    {
        Roles = roles ?? throw new ArgumentNullException(nameof(roles));
    }
}