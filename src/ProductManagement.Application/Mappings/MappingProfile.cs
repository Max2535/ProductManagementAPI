using AutoMapper;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Mappings;

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

        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryDetailDto>()
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<Category, CategoryDto>();

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.Name).ToList()));

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ItemCount,
                opt => opt.MapFrom(src => src.OrderItems.Count));

        CreateMap<OrderItem, OrderItemDto>();
    }
}