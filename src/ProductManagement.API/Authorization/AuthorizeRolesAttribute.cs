using Microsoft.AspNetCore.Authorization;

namespace ProductManagement.API.Authorization;

/// <summary>
/// Custom authorization attribute for multiple roles
/// Usage: [AuthorizeRoles("Admin", "Manager")]
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}