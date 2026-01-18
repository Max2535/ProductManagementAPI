using ProductManagement.Domain.Entities;

namespace ProductManagement.UnitTests.Builders;

/// <summary>
/// Test Data Builder Pattern for Category Entity
/// Makes test data creation clean and readable
/// </summary>
public class CategoryBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Category";
    private string _description = "Test Description";
    private Guid? _parentCategoryId = null;
    private string _createdBy = "TestUser";

    public CategoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CategoryBuilder WithParentCategoryId(Guid? parentCategoryId)
    {
        _parentCategoryId = parentCategoryId;
        return this;
    }

    public CategoryBuilder WithCreatedBy(string createdBy)
    {
        _createdBy = createdBy;
        return this;
    }

    public Category Build()
    {
        return Category.Create(
            _name,
            _description,
            _parentCategoryId,
            _createdBy);
    }
}
