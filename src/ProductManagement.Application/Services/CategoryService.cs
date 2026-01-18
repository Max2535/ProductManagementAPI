using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CategoryDetailDto>> CreateCategoryAsync(
        CreateCategoryRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating category: {Name}", request.Name);

            // Check if category with same name exists
            var existing = await _unitOfWork.Categories.FindAsync(
                c => c.Name == request.Name,
                cancellationToken);

            if (existing.Any())
            {
                return Result<CategoryDetailDto>.Failure($"Category '{request.Name}' already exists");
            }

            // Create category
            var category = Category.Create(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                createdBy);

            await _unitOfWork.Categories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Category created successfully: {CategoryId}", category.Id);

            var dto = _mapper.Map<CategoryDetailDto>(category);
            return Result<CategoryDetailDto>.Success(dto, "Category created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Result<CategoryDetailDto>.Failure("Failed to create category", ex.Message);
        }
    }

    public async Task<Result<CategoryDetailDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

            if (category == null)
            {
                return Result<CategoryDetailDto>.Failure("Category not found");
            }

            var dto = _mapper.Map<CategoryDetailDto>(category);
            return Result<CategoryDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category: {CategoryId}", id);
            return Result<CategoryDetailDto>.Failure("Failed to retrieve category", ex.Message);
        }
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            var dtos = _mapper.Map<List<CategoryDto>>(categories);
            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return Result<List<CategoryDto>>.Failure("Failed to retrieve categories", ex.Message);
        }
    }

    public async Task<Result<CategoryDetailDto>> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

            if (category == null)
            {
                return Result<CategoryDetailDto>.Failure("Category not found");
            }

            category.Update(request.Name, request.Description, updatedBy);

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CategoryDetailDto>(category);
            return Result<CategoryDetailDto>.Success(dto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {CategoryId}", id);
            return Result<CategoryDetailDto>.Failure("Failed to update category", ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
            return Result.Failure("Failed to delete category", ex.Message);
        }
    }
}