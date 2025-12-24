namespace ProductManagement.Domain.Entities;

public class Review : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;

    public int Rating { get; private set; } // 1-5
    public string Title { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public bool IsVerifiedPurchase { get; private set; }
    public ReviewStatus Status { get; private set; }

    public static Review Create(
        Guid productId,
        string userId,
        string userName,
        int rating,
        string title,
        string comment,
        bool isVerifiedPurchase)
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            UserName = userName,
            Rating = rating,
            Title = title,
            Comment = comment,
            IsVerifiedPurchase = isVerifiedPurchase,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        review.Validate();
        return review;
    }

    public void Approve(string approvedBy)
    {
        Status = ReviewStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = approvedBy;
    }

    public void Reject(string rejectedBy)
    {
        Status = ReviewStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = rejectedBy;
    }

    private void Validate()
    {
        if (Rating < 1 || Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5");

        if (string.IsNullOrWhiteSpace(Comment))
            throw new InvalidOperationException("Comment is required");
    }
}

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}