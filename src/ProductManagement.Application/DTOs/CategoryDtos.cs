namespace ProductManagement.Application.DTOs;

public record CreateCategoryRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? ParentCategoryId { get; init; }
}

public record UpdateCategoryRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record CategoryDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
}