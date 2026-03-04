using AutoMapper;
using Repositories.Models;
using Services.DTOs.RequestDTOs;
using Services.DTOs.ResponseDTOs;

namespace Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseDTO>().ReverseMap();
            CreateMap<User, AuthResponseDTO>();
            CreateMap<RegisterRequestDTO, User>();

            // Category Mappings
            CreateMap<Category, CategoryResponseDTO>().ReverseMap();
            CreateMap<CategoryRequestDTO, Category>();

            // Product Mappings
            CreateMap<Product, ProductResponseDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ReverseMap();
            CreateMap<ProductRequestDTO, Product>();

            // Cart Mappings
            CreateMap<Cart, CartResponseDTO>().ReverseMap();
            CreateMap<CartItem, CartItemResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ReverseMap();
            CreateMap<CartItemRequestDTO, CartItem>();

            // Notification Mappings
            CreateMap<Notification, NotificationResponseDTO>().ReverseMap();

            // Order Mappings
            CreateMap<Order, OrderResponseDTO>();
            CreateMap<OrderRequestDTO, Order>();

            // Payment Mappings
            CreateMap<Payment, PaymentResponseDTO>();
            CreateMap<PaymentRequestDTO, Payment>();

            // StoreLocation Mappings
            CreateMap<StoreLocation, StoreLocationDTO>();

            // Chat Mappings
            CreateMap<ChatMessage, ChatMessageResponseDTO>();
            CreateMap<ChatMessageRequestDTO, ChatMessage>();
        }
    }
}


