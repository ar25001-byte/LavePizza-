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
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Se gestiona mediante BCrypt en el servicio
    }
}
