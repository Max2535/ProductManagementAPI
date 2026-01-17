using ProductManagement.Domain.Entities;

namespace ProductManagement.UnitTests.Builders;

/// <summary>
/// Test Data Builder Pattern
/// Makes test data creation clean and readable
/// </summary>
public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Product";
    private string _description = "Test Description";
    private string _sku = "TEST-001";
    private decimal _price = 99.99m;
    private int _stockQuantity = 100;
    private Guid _categoryId = Guid.NewGuid();
    private int _minimumStockLevel = 10;
    private string _createdBy = "TestUser";

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithSKU(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithStock(int quantity)
    {
        _stockQuantity = quantity;
        return this;
    }

    public ProductBuilder WithLowStock()
    {
        _stockQuantity = 5;
        _minimumStockLevel = 10;
        return this;
    }

    public ProductBuilder WithNoStock()
    {
        _stockQuantity = 0;
        return this;
    }

    public ProductBuilder WithCategory(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public Product Build()
    {
        var product = Product.Create(
            _name,
            _description,
            _sku,
            _price,
            _stockQuantity,
            _categoryId,
            _createdBy,
            _minimumStockLevel);

        // Use reflection to set the ID if it was explicitly set via WithId()
        var idProperty = typeof(Product).GetProperty("Id");
        if (idProperty != null)
        {
            idProperty.SetValue(product, _id);
        }

        return product;
    }
}