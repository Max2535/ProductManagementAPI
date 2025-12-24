namespace ProductManagement.Domain.Entities;

/// <summary>
/// Base entity class สำหรับ common properties
/// ใช้ Generic ลดการ duplicate code
/// </summary>
public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } // Soft delete
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}