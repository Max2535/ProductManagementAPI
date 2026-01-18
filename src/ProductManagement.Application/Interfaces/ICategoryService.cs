using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDetailDto>> CreateCategoryAsync(
        CreateCategoryRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<Result<CategoryDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<List<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<CategoryDetailDto>> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);
}