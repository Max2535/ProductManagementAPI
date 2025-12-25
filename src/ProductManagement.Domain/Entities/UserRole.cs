namespace ProductManagement.Domain.Entities;

/// <summary>
/// Many-to-Many relationship between User and Role
/// </summary>
public class UserRole : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }
}