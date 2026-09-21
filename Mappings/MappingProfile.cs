using AutoMapper;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeos de Productos
        CreateMap<Product, ProductResponse>();
        CreateMap<ProductRequest, Product>();

        // Mapeos de Usuarios
        CreateMap<User, UserResponse>();
        CreateMap<UserRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // Mapeos de Categorías
        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();

        // Mapeos de Direcciones
        CreateMap<Address, AddressDto>();
        CreateMap<AddressCreateRequest, Address>();

        // Mapeos de Métodos de Pago
        CreateMap<PaymentMethod, PaymentMethodDto>();
        CreateMap<PaymentMethodCreateRequest, PaymentMethod>();

        // Mapeos de Items de Pedido
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice));

        // Mapeos de Pedidos
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.TrackingHistory, opt => opt.Ignore()); // TrackingHistory se calcula en la lógica de negocio según el estado
    }
}
