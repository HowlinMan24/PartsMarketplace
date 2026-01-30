using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Category, CategoryDto>();

        CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : ""))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserId));

        CreateMap<CreateListingDto, Listing>();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserId));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ListingTitle, opt => opt.MapFrom(src => src.Listing.Title));
    }
}
