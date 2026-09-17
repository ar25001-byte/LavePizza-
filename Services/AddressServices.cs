using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeraPizza.DTOs;
using VeraPizza.Models;

namespace VeraPizza.Services;

public class AddressServices : IAddressServices
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AddressServices(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(int userId)
    {
        var addresses = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AddressDto>>(addresses);
    }

    public async Task<AddressDto?> GetAddressByIdAsync(int id, int userId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null) return null;
        return _mapper.Map<AddressDto>(address);
    }

    public async Task<AddressDto> CreateAddressAsync(int userId, AddressCreateRequest request)
    {
        var address = _mapper.Map<Address>(request);
        address.UserId = userId;

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    public async Task<bool> DeleteAddressAsync(int id, int userId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null) return false;

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return true;
    }
}
