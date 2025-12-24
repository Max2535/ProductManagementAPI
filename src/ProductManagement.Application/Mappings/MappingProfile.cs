using AutoMapper;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Mappings;

/// <summary>
/// AutoMapper configuration
/// Best Practice: Centralized mapping configuration
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count));

        CreateMap<Product, ProductSummaryDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PrimaryImageUrl,
                opt => opt.MapFrom(src => src.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl));

        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<Category, CategoryDto>();

        // Request to Entity mappings (if needed for simple cases)
        // Usually we create entities using factory methods
    }
}