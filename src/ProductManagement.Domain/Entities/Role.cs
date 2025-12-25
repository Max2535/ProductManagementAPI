namespace ProductManagement.Domain.Entities;

public class Role : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Navigation property
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Predefined roles
    public static class Names
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Customer = "Customer";
    }

    public static Role Create(string name, string description, string createdBy)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        role.Validate();
        return role;
    }

    public void Update(string name, string description, string updatedBy)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Role name is required");
    }

    // Seed default roles
    public static IEnumerable<Role> GetDefaultRoles()
    {
        var createdAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new[]
        {
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = Names.Admin,
                Description = "Administrator with full access",
                CreatedAt = createdAt,
                CreatedBy = "System",
                IsDeleted = false
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = Names.Manager,
                Description = "Manager with limited administrative access",
                CreatedAt = createdAt,
                CreatedBy = "System",
                IsDeleted = false
            },
            new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = Names.Customer,
                Description = "Regular customer",
                CreatedAt = createdAt,
                CreatedBy = "System",
                IsDeleted = false
            }
        };
    }
}