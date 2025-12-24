namespace ProductManagement.Domain.Entities;

public class ProductImage : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    public static ProductImage Create(Guid productId, string imageUrl, bool isPrimary, string createdBy)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void SetAsNonPrimary()
    {
        IsPrimary = false;
    }
}