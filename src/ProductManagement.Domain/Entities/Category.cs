namespace ProductManagement.Domain.Entities;

public class Category : BaseEntity<Guid>
{
    private readonly List<Product> _products = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // Self-referencing for hierarchical categories
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public static Category Create(string name, string description, Guid? parentCategoryId, string createdBy)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Slug = GenerateSlug(name),
            ParentCategoryId = parentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        category.Validate();
        return category;
    }

    public void Update(string name, string description, string updatedBy)
    {
        Name = name;
        Description = description;
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        Validate();
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Category name is required");
    }
}