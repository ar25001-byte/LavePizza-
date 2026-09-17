using VeraPizza.DTOs;

namespace VeraPizza.Services;

public interface IAddressServices
{
    Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId);
    Task<AddressDto?> GetAddressByIdAsync(int id, int userId);
    Task<AddressDto> CreateAddressAsync(int userId, AddressCreateRequest request);
    Task<bool> DeleteAddressAsync(int id, int userId);
}
